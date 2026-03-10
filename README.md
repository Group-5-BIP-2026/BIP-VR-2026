# BIP-VR-2026

Unity version: 6.3 LTS (6000.3.10f1)

## Unity "UnityYAMLMerge" setup

After cloning the repository, add the following entries manually to your local repository Git config (`.git/config`) so that scenes and prefabs merges using the `UnityYAMLMerge` tool from Unity.

```ini
[merge]
    tool = unityyamlmerge
    tool = unityyamlmerge.trustExitCode
[mergetool "unityyamlmerge"]
    trustExitCode = true
    cmd = 'C:/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$BASE" "$MERGED"
```

This is a local-only Git setting, so each contributor needs to add it on their own machine.
