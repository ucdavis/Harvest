namespace Anlab.Core.Models.SlothModels
{
    public static class TransferStatusCodes
    {
        //TODO: Add as we figure out what they want/need
        public const string Created         = "Created";          //Created by user, not sent and is still editable
        public const string Submitted       = "Submitted";        //Sent and no longer editable
        public const string Complete        = "Complete";         //Money has moved

        public static readonly string[] All = { Created, Submitted, Complete };
    }
}
