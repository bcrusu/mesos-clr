﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using com.bcrusu.mesosclr.Rendler.Executors.Messages;
using mesos;

namespace com.bcrusu.mesosclr.Rendler
{
    internal class RendlerScheduler : IScheduler
    {
        private const double RenderCpus = 1d;
        private const double RenderMem = 128d;
        private const double CrawlCpus = 0.5d;
        private const double CrawlMem = 64d;

        private readonly string _outputDir;
        private readonly int _maxTasksToRun;

        private int _launchedTasks;
        private int _finishedTasksCount;
        private readonly ConcurrentQueue<string> _crawlQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _renderQueue = new ConcurrentQueue<string>();
        private readonly ISet<string> _crawled = new HashSet<string>();

        private readonly ConcurrentDictionary<string, string> _urlToFileMap = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, List<string>> _edgesMap = new ConcurrentDictionary<string, List<string>>();

        public RendlerScheduler(string startUrl, string outputDir, int maxTasksToRun = 100)
        {
            if (startUrl == null) throw new ArgumentNullException(nameof(startUrl));
            if (outputDir == null) throw new ArgumentNullException(nameof(outputDir));
            _outputDir = outputDir;
            _maxTasksToRun = maxTasksToRun;

            _crawlQueue.Enqueue(startUrl);
            _renderQueue.Enqueue(startUrl);
        }

        public void Registered(ISchedulerDriver driver, FrameworkID frameworkId, MasterInfo masterInfo)
        {
            Console.WriteLine($"Registered with Mesos master. FrameworkId='{frameworkId.value}'.");
        }

        public void Reregistered(ISchedulerDriver driver, MasterInfo masterInfo)
        {
        }

        public void ResourceOffers(ISchedulerDriver driver, IEnumerable<Offer> offers)
        {
            foreach (var offer in offers)
            {
                var tasks = new List<TaskInfo>();
                var resourcesCounter = new ResourcesCounter(offer);
                var done = true;
                do
                {
                    string renderUrl;
                    if (resourcesCounter.HasRenderTaskResources() && _renderQueue.TryDequeue(out renderUrl))
                    {
                        tasks.Add(GetRenderTaskInfo(offer, ++_launchedTasks, renderUrl));
                        resourcesCounter.SubstractRenderResources();
                        done = false;
                    }

                    string crawlUrl;
                    if (resourcesCounter.HasCrawlTaskResources() && _crawlQueue.TryDequeue(out crawlUrl))
                    {
                        tasks.Add(GetCrawlTaskInfo(offer, ++_launchedTasks, crawlUrl));
                        resourcesCounter.SubstractCrawlResources();
                        _crawled.Add(crawlUrl);
                        done = false;
                    }
                } while (!done);

                if (tasks.Any())
                    driver.LaunchTasks(new[] { offer.id }, tasks);
                else
                    driver.DeclineOffer(offer.id);
            }
        }

        public void OfferRescinded(ISchedulerDriver driver, OfferID offerId)
        {
        }

        public void StatusUpdate(ISchedulerDriver driver, TaskStatus status)
        {
            if (status.state.IsTerminal())
            {
                Console.WriteLine($"Status update: task '{status.task_id}' has terminated with state '{status.state}'.");
                var finishedTasksCount = Interlocked.Increment(ref _finishedTasksCount);

                if (finishedTasksCount == _maxTasksToRun)
                {
                    Console.WriteLine("Reached the max number of tasks to run. Stopping...");

                    var dotWritePath = Path.Combine(_outputDir, "result.dot");
                    DotHelper.Write(dotWritePath, _edgesMap, _urlToFileMap);
                    driver.Stop();
                }
            }
            else
            {
                Console.WriteLine($"Status update: task '{status.task_id}' is in state '{status.state}'.");
            }
        }

        public void FrameworkMessage(ISchedulerDriver driver, ExecutorID executorId, SlaveID slaveId, byte[] data)
        {
            var message = JsonHelper.Deserialize<Message>(data);
            switch (message.Type)
            {
                case "CrawlResult":
                    var crawlResult = JsonHelper.Deserialize<CrawlResultMessage>(message.Body);

                    foreach (var link in crawlResult.Links)
                    {
                        if (_crawled.Contains(link))
                            continue;

                        _crawlQueue.Enqueue(link);
                        _renderQueue.Enqueue(link);
                    }

                    // update edges: url -> links
                    var edges = _edgesMap.GetOrAdd(crawlResult.Url, x => new List<string>());
                    edges.AddRange(crawlResult.Links);

                    // empty edge list for links
                    foreach (var link in crawlResult.Links)
                        _edgesMap.GetOrAdd(link, x => new List<string>());
                    break;
                case "RenderResult":
                    var renderResult = JsonHelper.Deserialize<RenderResultMessage>(message.Body);
                    _urlToFileMap[renderResult.Url] = renderResult.FileName;
                    break;
                default:
                    Console.WriteLine($"Unrecognized message type: '{message.Type}'");
                    break;
            }
        }

        public void Disconnected(ISchedulerDriver driver)
        {
        }

        public void SlaveLost(ISchedulerDriver driver, SlaveID slaveId)
        {
        }

        public void ExecutorLost(ISchedulerDriver driver, ExecutorID executorId, SlaveID slaveId, int status)
        {
        }

        public void Error(ISchedulerDriver driver, string message)
        {
            Console.WriteLine($"Error: '{message}'.");
        }

        private TaskInfo GetRenderTaskInfo(Offer offer, int uniqueId, string url)
        {
            return new TaskInfo
            {
                name = "Rendler.Render_" + uniqueId,
                task_id = new TaskID { value = uniqueId.ToString() },
                slave_id = offer.slave_id,
                resources =
                {
                    new Resource {name = "cpus", type = Value.Type.SCALAR, scalar = new Value.Scalar {value = RenderCpus}},
                    new Resource {name = "mem", type = Value.Type.SCALAR, scalar = new Value.Scalar {value = RenderMem}}
                },
                executor = new ExecutorInfo
                {
                    executor_id = new ExecutorID { value = "RenderExecutor" },
                    command = new CommandInfo { value = "mono rendler.exe -executor=render" },
                    data = Encoding.UTF8.GetBytes(_outputDir)
                },
                data = Encoding.UTF8.GetBytes(url)
            };
        }

        private static TaskInfo GetCrawlTaskInfo(Offer offer, int uniqueId, string url)
        {
            return new TaskInfo
            {
                name = "Rendler.Crawl_" + uniqueId,
                task_id = new TaskID { value = uniqueId.ToString() },
                slave_id = offer.slave_id,
                resources =
                {
                    new Resource {name = "cpus", type = Value.Type.SCALAR, scalar = new Value.Scalar {value = CrawlCpus}},
                    new Resource {name = "mem", type = Value.Type.SCALAR, scalar = new Value.Scalar {value = CrawlMem}}
                },
                executor = new ExecutorInfo
                {
                    executor_id = new ExecutorID { value = "CrawlExecutor" },
                    command = new CommandInfo { value = "mono rendler.exe -executor=crawl" }
                },
                data = Encoding.UTF8.GetBytes(url)
            };
        }

        private class ResourcesCounter
        {
            private double _cpus;
            private double _mem;

            public ResourcesCounter(Offer offer)
            {
                var cpusResource = offer.resources.SingleOrDefault(x => x.name == "cpus");
                var memResource = offer.resources.SingleOrDefault(x => x.name == "mem");
                _cpus = cpusResource?.scalar.value ?? 0d;
                _mem = memResource?.scalar.value ?? 0d;
            }

            private void Substract(double cpus, double mem)
            {
                _cpus = _cpus - cpus;
                _mem = _mem - mem;
            }

            public bool HasRenderTaskResources()
            {
                return HasResources(RenderCpus, RenderMem);
            }

            public bool HasCrawlTaskResources()
            {
                return HasResources(CrawlCpus, CrawlMem);
            }

            public void SubstractRenderResources()
            {
                Substract(RenderCpus, RenderMem);
            }

            public void SubstractCrawlResources()
            {
                Substract(CrawlCpus, CrawlMem);
            }

            private bool HasResources(double cpus, double mem)
            {
                return _cpus >= cpus && _mem >= mem;
            }
        }
    }
}