using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NetProtocol.MessageProcessors;
using NUnit.Framework;

using MessageType = cg.network.protocol.MessageType;

namespace NetProtocol.Test
{
    internal class PackageQueueProcessorTest
    {
        [Test]
        public void AddTest()
        {
            ManualResetEventSlim signalevent = new ManualResetEventSlim(false);
            int totalCount = 0;

            Action<ReceiveMessageEventArgs> action = (ReceiveMessageEventArgs args) => 
            {
                Assert.IsNotNull(args);

                totalCount++;
                signalevent.Set();
            };

            var processor = new PackageQueueProcessor<ReceiveMessageEventArgs>(action);
            processor.Start();
            processor.Add(new ReceiveMessageEventArgs(MessageType.CM_SYSTEM_INFO, new byte[] { 0, 1, 2 }));
            processor.Add(null);

            signalevent.Wait(100);
            Assert.AreEqual(1, totalCount);

            processor.Stop();
            Assert.IsFalse(processor.IsActive);

            // Повторный запуск после остановки
            processor.Start();
            Assert.IsTrue(processor.IsActive);

            processor.Stop();
            Assert.IsFalse(processor.IsActive);
        }

        [Test]
        public void EmptyActionTest()
        {
            var processor = new PackageQueueProcessor<ReceiveMessageEventArgs>(null);

            processor.Start();
            processor.Add(new ReceiveMessageEventArgs(MessageType.CS_SUBSCRIPTION_LIMIT_CONTINUE, new byte[] { 0, 1, 2 }));
            processor.Add(null);

            Assert.IsFalse(processor.IsActive);
            Assert.AreEqual(0, processor.Count);
            processor.Stop();
        }

        [Test]
        public void InvalidBehaviorTest()
        {
            int totalCount = 0;
            Action<ReceiveMessageEventArgs> action = (ReceiveMessageEventArgs args) =>
            {
                Assert.IsNotNull(args);
                totalCount++;
            };

            var processor = new PackageQueueProcessor<ReceiveMessageEventArgs>(action);

            // Проверка неверного использования
            processor.Stop();
            processor.Start();
            processor.Start();
        }

    }
}
