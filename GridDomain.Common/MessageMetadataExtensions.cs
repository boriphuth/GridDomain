using System;

namespace GridDomain.Common
{
    public static class MessageMetadataExtensions
    {
        public static MessageMetadata CreateChild(this IMessageMetadata metadata, 
            Guid messageId,
            params ProcessEntry[] process)
        {
            return MessageMetadata.CreateFrom(messageId, metadata, process);
        }    
    }
}