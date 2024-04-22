using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF.Loader;

namespace Mona.SDK.Brains.Core.Utils
{
	public class StreamLoader : IDataLoader
	{
		private Stream _stream;

		public StreamLoader(Stream stream)
		{
			_stream = stream;
		}
		public async Task<Stream> LoadStreamAsync(string relativeFilePath)
		{
			return _stream;
		}
	}
}
