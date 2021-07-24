# Template Better

## What?

Template Better is a Unity editor script that allows you to create richer, cascading C# script templates.

This was based on this Unity forum post: https://forum.unity.com/threads/c-script-template-how-to-make-custom-changes.273191/#post-1806467

## How?

Add this repo as a package to your project. Template Better will pick up C# templates in your `ScriptTemplates` directory.

Given this example template (`ScriptTemplates/Template.cs.txt` or `ScriptTemplates/81-C# Script-NewBehaviourScript.cs.txt`):

```
//
// #SCRIPTNAME#.cs
// #PROJECTNAME#
//
// Created by #AUTHOR# on #DAY#/#MONTH#/#YEAR#.
// Copyright (c) #YEAR# #COMPANY# All rights reserved.
//

using UnityEngine;

public class #SCRIPTNAME# : MonoBehaviour {
	void Start() {
		#NOTRIM#
	}

	void Update() {
		#NOTRIM#
	}
}
```

the following C# file will be generated for you:

```
//
// NewBehaviourScript.cs
// MyAmazingGame
//
// Created by ben on 24/07/2021.
// Copyright (c) 2021 DefaultCompany All rights reserved.
//

using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
	void Start() {
		
	}

	void Update() {
		
	}
}
```

If you have a directory structure like this:

```
Assets
  ├── ScriptTemplates
  │   └── Template.cs.txt
  ├── Scripts
  │   └── TestOne.cs
  ├── SubFolder
  │   ├── ScriptTemplates
  │   │   └── Template.cs.txt
  │   └── Scripts
  │        └── TestTwo.cs
```

Then `Assets/Scripts/TestOne.cs` would use `Assets/ScriptTemplates/Template.cs.txt`, but `Assets/SubFolder/Scripts/TestTwo.cs` would use `Assets/SubFolder/ScriptTemplates/Template.cs.txt`.

There is some API to allow you to bind your own tags, take a look at `TemplateBetter.RegisterSymbolTextGenerator(string symbol, SymbolTextGenerator textGenerator);`.

Please note: Currently `#ROOTNAMESPACEBEGIN#` is not supported as it's a little more tricky to do this.

## Roadmap

This script is offered as a proof of concept, without any support. If you would like to sponsor it then please let me know: http://twitter.com/btsherratt/.