/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus Utilities SDK License Version 1.31 (the "License"); you may not use
the Utilities SDK except in compliance with the License, which is provided at the time of installation
or download, or which otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at
https://developer.oculus.com/licenses/utilities-1.31

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoadAttribute]
public class OculusSampleFrameworkUtil
{
  static OculusSampleFrameworkUtil()
  {
#if UNITY_2017_2_OR_NEWER
    EditorApplication.playModeStateChanged += HandlePlayModeState;
#else
    EditorApplication.playmodeStateChanged += () =>
    {
      if (EditorApplication.isPlaying)
      {
        OVRPlugin.SendEvent("load", OVRPlugin.wrapperVersion.ToString(), "sample_framework");
      }
    };
#endif
	}

#if UNITY_2017_2_OR_NEWER
	private static void HandlePlayModeState(PlayModeStateChange state)
  {
    if (state == PlayModeStateChange.EnteredPlayMode)
    {
      if (OVRPlugin.wrapperVersion != null)
      {
        OVRPlugin.SendEvent("load", OVRPlugin.wrapperVersion.ToString(), "sample_framework");
      }
    }
  }
#endif
}

#endif
