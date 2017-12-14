namespace ICSharpCode.SharpZipLib.Zip.Compression 
{
	
	/// <summary>
	/// This class stores the pending output of the Deflater.
	/// 
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	public class DeflaterPending : PendingBuffer
	{
		public DeflaterPending() : base(DeflaterConstants.PENDING_BUF_SIZE)
		{
		}
	}
}
