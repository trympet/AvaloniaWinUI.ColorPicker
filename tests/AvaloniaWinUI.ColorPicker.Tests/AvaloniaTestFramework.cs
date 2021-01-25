// https://github.com/kekekeks/Avalonia-unit-testing-with-headless-platform/blob/master/HeadlessTests/AvaloniaTestFramework.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Headless;
using Avalonia.Threading;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("AvaloniaWinUI.ColorPicker.Tests.AvaloniaTestFramework", "AvaloniaWinUI.ColorPicker.Tests")]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false, MaxParallelThreads = 1)]
namespace AvaloniaWinUI.ColorPicker.Tests
{
    public class AvaloniaTestFramework : XunitTestFramework
    {
        public AvaloniaTestFramework(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new Executor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        internal class Executor : XunitTestFrameworkExecutor
        {
            public Executor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider,
                IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider,
                diagnosticMessageSink)
            {
            }

            protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases,
                IMessageSink executionMessageSink,
                ITestFrameworkExecutionOptions executionOptions)
            {
                executionOptions.SetValue("xunit.execution.DisableParallelization", false);
                using var assemblyRunner = new Runner(
                    TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink,
                    executionOptions);
                await assemblyRunner.RunAsync().ConfigureAwait(true);
            }
        }

        internal class Runner : XunitTestAssemblyRunner
        {
            public Runner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases,
                IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink,
                ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink,
                executionMessageSink, executionOptions)
            {
            }


            protected override void SetupSyncContext(int maxParallelThreads)
            {
                var tcs = new TaskCompletionSource<SynchronizationContext>();
                new Thread(() =>
                {
                    try
                    {
                        App.BuildAvaloniaApp()
                            .UseHeadless()
                            .SetupWithoutStarting();
                        tcs.SetResult(SynchronizationContext.Current!);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        tcs.SetException(e);
                    }
                    Dispatcher.UIThread.MainLoop(CancellationToken.None);
                })
                {
                    IsBackground = true
                }.Start();

                SynchronizationContext.SetSynchronizationContext(tcs.Task.Result);
            }


        }
    }
}
