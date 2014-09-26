using System;
using System.Linq;

namespace ServiceControl.Infrastructure.ElasticSearch {
	using MessageAuditing;
	using MessageFailures;
	using Nest;
	using NServiceBus;
	using NServiceBus.Logging;
	using ServiceBus.Management.Infrastructure.Settings;

	public static class ElasticSearchWrapper {
		static readonly ILog Logger = LogManager.GetLogger(typeof(ElasticSearchWrapper));
		public static void IndexAuditMessage(ProcessedMessage auditMessage) {
			try {
				var elasticSearchHost = "http://" + Settings.ElasticSearchHostName + ":" + Settings.ElasticSearchPort;
				var node = new Uri(elasticSearchHost);
				var settings = new ConnectionSettings(node);

				var client = new ElasticClient(settings);
				var indexName = Settings.ElasticSearchIndex + "-" + DateTime.Now.Year.ToString("d2") + "." + DateTime.Now.Month.ToString("d2") + "." + DateTime.Now.Day.ToString("d2");

				if (!client.IndexExists(indexName).Exists) {
					Logger.Info("Creating index: " + indexName);
					client.CreateIndex(indexName);
				}
				var unformattedDate = auditMessage.Headers["NServiceBus.TimeSent"];
				var utcDateTime = DateTimeExtensions.ToUtcDateTime(unformattedDate);
				var timestamp = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
				auditMessage.Timestamp = utcDateTime;
				var index = client.Index(auditMessage, i => i.Index(indexName).Timestamp(timestamp));

				if (!index.Created) {
					Logger.Error("Unable to index auditMessage. UniqueMessageId: " + auditMessage.UniqueMessageId);
				}
			}
			catch (Exception ex) {
				Logger.Error("Exception occured in ElasticSearch logic", ex);
			}
		}

		public static void IndexFailedMessage(FailedMessage failedMessage) {
			try {
				var elasticSearchHost = "http://" + Settings.ElasticSearchHostName + ":" + Settings.ElasticSearchPort;
				var node = new Uri(elasticSearchHost);
				var settings = new ConnectionSettings(node);

				var client = new ElasticClient(settings);
				var indexName = Settings.ElasticSearchIndex + "-" + DateTime.Now.Year.ToString("d2") + "." + DateTime.Now.Month.ToString("d2") + "." + DateTime.Now.Day.ToString("d2");

				if (!client.IndexExists(indexName).Exists) {
					Logger.Info("Creating index: " + indexName);
					client.CreateIndex(indexName);
				}
				
				var unformattedDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

				if (failedMessage.ProcessingAttempts != null && failedMessage.ProcessingAttempts.Count > 0)
				{
					unformattedDate = failedMessage.ProcessingAttempts.Last().Headers["NServiceBus.TimeSent"];
				}
				var utcDateTime = DateTimeExtensions.ToUtcDateTime(unformattedDate);
				var timestamp = utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
				failedMessage.Timestamp = utcDateTime;
				var index = client.Index(failedMessage, i => i.Index(indexName).Timestamp(timestamp));

				if (!index.Created) {
					Logger.Error("Unable to index failedMessage. UniqueMessageId: " + failedMessage.UniqueMessageId);
				}
			}
			catch (Exception ex) {
				Logger.Error("Exception occured in ElasticSearch logic", ex);
			}
		}
	}
}
