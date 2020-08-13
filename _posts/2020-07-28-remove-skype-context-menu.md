---
title:  "Remove skype context menu with powershell"
date:   2020-07-28 14:47:00 +01:00
categories: powershell
tags: 
  - powershell
  - skype
  - registry
  - windows terminal
---

Although powershell is enabled by default when installing Windows Terminal, it seems the development team have overlooked the scenario where you want to run a script as Admin and don't want to context switch. Adding the below entry into your terminal settings json file will give you a powershell admin prompt.

Enable run powershell as admin (in windows terminal):

{% highlight json %}
// https://github.com/microsoft/terminal/issues/632
{
    "guid": "{42f54c7a-0364-4ce2-bbb6-81efb79ec97f}",
    "hidden": false,
    "name": "Powershell (Admin)",
    "commandline": "%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe -command Start-Process -Verb RunAs \"shell:appsFolder\\Microsoft.WindowsTerminal_8wekyb3d8bbwe!App\"",
    "icon": "ms-appx:///Images/Square44x44Logo.targetsize-32.png"
}
{% endhighlight %}

Remove that pesky right click context menu:

{% highlight powershell %}
$subPath = "HKLM:\SOFTWARE\Classes\packagedcom\package\"

$skypePackageId = 
    dir $subPath | 
    ? { $_.Name -like '*skypeapp*' } | 
    select-object -Property name | 
    % { $_.Name } | 
    split-path -Leaf

$withoutClassId = "$subpath" + "$skypePackageId\class\"

$classId = 
    dir $withoutClassId | 
    ? { $_.Property -like '*DllPath*' } | 
    % { $_.Name } |
    split-path -leaf    

$pathWithClassId = "$subpath" + "$skypePackageId\class\$classId"
if((Test-Path $pathWithClassId) -and ($classId -ne $null))
{
    Write-Host "Found $pathWithClassId"
    pushd $withoutClassId
    remove-item $classId -confirm
    popd
}
{% endhighlight %}