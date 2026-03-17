using System.Collections.Immutable;
using System.Text.Json;

namespace AiChatClient.Maui;

public class TrainedFileNameService(IPreferences preferences)
{
	const string _trainedFilesKey = "TrainedFiles";

#if IOS || ANDROID
	readonly List<string> _inMemoryFileNameList = [];
#else
	readonly IPreferences _preferences = preferences;
#endif

	public ImmutableList<string> TrainedFileNames
	{
		get
		{
#if IOS || ANDROID
			return [.._inMemoryFileNameList];
#else
			var serializedTrainedFiles = _preferences.Get<string>(_trainedFilesKey, "[]");
			return JsonSerializer.Deserialize<ImmutableList<string>>(serializedTrainedFiles)
			       ?? throw new InvalidOperationException("");
#endif
		}
	}

	public void AddFilename(string fileName)
	{
#if IOS || ANDROID
		_inMemoryFileNameList.Add(fileName);
#else
		var updatedTrainedFileNamesList = TrainedFileNames.Add(fileName);
		_preferences.Set(_trainedFilesKey, updatedTrainedFileNamesList);
#endif
	}
}