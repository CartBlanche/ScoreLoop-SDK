using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libCoreSocial.iphoneos.a", LinkTarget.ArmV6 | LinkTarget.ArmV7, ForceLoad = true)]
