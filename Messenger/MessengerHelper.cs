using UnityEngine;

//This manager will ensure that the messenger's eventTable will be cleaned up upon loading of a new level.
public sealed class MessengerHelper : MonoBehaviour {
	void Awake ()
	{
		DontDestroyOnLoad(gameObject);	
	}
	
	//Clean up eventTable every time a new level loads.
	//public void OnLevelWasLoaded(int unused) {
	//	Debug.Log( "OnLevelWasLoaded" );
	//	Messenger.Cleanup();
	//}
}