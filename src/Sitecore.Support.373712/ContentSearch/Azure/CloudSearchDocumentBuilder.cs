namespace Sitecore.Support.ContentSearch.Azure
{
    using Sitecore.ContentSearch;
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;

    public class CloudSearchDocumentBuilder : Sitecore.ContentSearch.Azure.CloudSearchDocumentBuilder
    {
        private readonly CultureInfo culture;

        public CloudSearchDocumentBuilder(IIndexable indexable, IProviderUpdateContext context)
        : base(indexable, context)
        {
            culture = indexable?.Culture;
        }

        protected override void AddItemFields()
        {
            try
            {
                VerboseLogging.CrawlingLogDebug(() => "AddItemFields start");
                if (Options.IndexAllFields)
                {
                    Indexable.LoadAllFields();
                }
                if (IsParallel)
                {
                    ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();
                    ParallelForeachProxy.ForEach(Indexable.Fields, ParallelOptions, delegate (IIndexableDataField f)
                    {
                        try
                        {
                            CheckAndAddField(Indexable, f);
                        }
                        catch (Exception item)
                        {
                            exceptions.Enqueue(item);
                        }
                    });
                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException(exceptions);
                    }
                }
                else
                {
                    foreach (IIndexableDataField field in Indexable.Fields)
                    {
                        CheckAndAddField(Indexable, field);
                    }
                }
            }
            finally
            {
                VerboseLogging.CrawlingLogDebug(() => "AddItemFields End");
            }
        }
    }
}