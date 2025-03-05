using System;
using SD = System.Drawing;

using Rhino;
using Grasshopper.Kernel;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class AssemblyInfo : GH_AssemblyInfo
  {
    public override Guid Id { get; } = new Guid("02fe8045-bbb8-4621-93a2-dc314e0061f6");

    public override string AssemblyName { get; } = "Assembler.GH";
    public override string AssemblyVersion { get; } = "1.0.2.9119";
    public override string AssemblyDescription { get; } = "";
    public override string AuthorName { get; } = "Weiching Chen";
    public override string AuthorContact { get; } = "wc683@cornell.edu";
    public override GH_LibraryLicense AssemblyLicense { get; } = GH_LibraryLicense.unset;
    public override SD.Bitmap AssemblyIcon { get; } = ProjectComponentPlugin.PluginIcon;
  }

  public class ProjectComponentPlugin : GH_AssemblyPriority
  {
    public static SD.Bitmap PluginIcon { get; }
    public static SD.Bitmap PluginCategoryIcon { get; }

    static readonly Guid s_rhinocode = new Guid("c9cba87a-23ce-4f15-a918-97645c05cde7");
    static readonly Type s_invokeContextType = default;
    static readonly dynamic s_projectServer = default;
    static readonly object s_project = default;

    static readonly Guid s_projectId = new Guid("02fe8045-bbb8-4621-93a2-dc314e0061f6");
    static readonly string s_projectData = "ewogICJpZCI6ICIwMmZlODA0NS1iYmI4LTQ2MjEtOTNhMi1kYzMxNGUwMDYxZjYiLAogICJpZGVudGl0eSI6IHsKICAgICJuYW1lIjogIkFzc2VtYmxlciIsCiAgICAidmVyc2lvbiI6ICIxLjAuMi1iZXRhIiwKICAgICJwdWJsaXNoZXIiOiB7CiAgICAgICJlbWFpbCI6ICJ3YzY4M0Bjb3JuZWxsLmVkdSIsCiAgICAgICJuYW1lIjogIldlaWNoaW5nIENoZW4iCiAgICB9LAogICAgImNvcHlyaWdodCI6ICJDb3B5cmlnaHQgXHUwMEE5IDIwMjQgV2VpY2hpbmcgQ2hlbiwgWGlhb21hbiBZYW5nLCBDYXJyaWUgV2FuZyIsCiAgICAibGljZW5zZSI6ICJNSVQiCiAgfSwKICAic2V0dGluZ3MiOiB7CiAgICAiYnVpbGRQYXRoIjogImZpbGU6Ly8vVXNlcnMvanVsaWEvRGVza3RvcC9Db3JuZWxsMjRGYWxsL2Rlc2lnbjYyOTdmaW5hbC9idWlsZC9yaDgtbWFjIiwKICAgICJidWlsZFRhcmdldCI6IHsKICAgICAgImFwcE5hbWUiOiAiUmhpbm8zRCIsCiAgICAgICJhcHBWZXJzaW9uIjogewogICAgICAgICJtYWpvciI6IDgKICAgICAgfSwKICAgICAgImFwcE9TIjogIm1hY09TIiwKICAgICAgInRpdGxlIjogIlJoaW5vM0QgKDguKiAtIG1hY09TKSIsCiAgICAgICJzbHVnIjogInJoOC1tYWMiCiAgICB9LAogICAgInB1Ymxpc2hUYXJnZXQiOiB7CiAgICAgICJ0aXRsZSI6ICJNY05lZWwgWWFrIFNlcnZlciIKICAgIH0KICB9LAogICJjb2RlcyI6IFtdCn0=";
    static readonly string _iconData = "[[ASSEMBLY-ICON]]";

    static ProjectComponentPlugin()
    {
      s_projectServer = ProjectInterop.GetProjectServer();
      if (s_projectServer is null)
      {
        RhinoApp.WriteLine($"Error loading Grasshopper plugin. Missing Rhino3D platform");
        return;
      }

      // get project
      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["projectAssembly"] = typeof(ProjectComponentPlugin).Assembly;
      dctx.Inputs["projectId"] = s_projectId;
      dctx.Inputs["projectData"] = s_projectData;

      object project = default;
      if (s_projectServer.TryInvoke("plugins/v1/deserialize", dctx)
            && dctx.Outputs.TryGet("project", out project))
      {
        // server reports errors
        s_project = project;
      }

      // get icons
      if (!_iconData.Contains("ASSEMBLY-ICON"))
      {
        dynamic ictx = ProjectInterop.CreateInvokeContext();
        ictx.Inputs["iconData"] = _iconData;
        SD.Bitmap icon = default;
        if (s_projectServer.TryInvoke("plugins/v1/icon/gh/assembly", ictx)
              && ictx.Outputs.TryGet("icon", out icon))
        {
          // server reports errors
          PluginIcon = icon;
        }

        if (s_projectServer.TryInvoke("plugins/v1/icon/gh/category", ictx)
              && ictx.Outputs.TryGet("icon", out icon))
        {
          // server reports errors
          PluginCategoryIcon = icon;
        }
      }
    }

    public override GH_LoadingInstruction PriorityLoad()
    {
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Assembler", "Assembler"[0]);

      if (PluginCategoryIcon != null)
        Grasshopper.Instances.ComponentServer.AddCategoryIcon("Assembler", PluginCategoryIcon);

      return GH_LoadingInstruction.Proceed;
    }

    public static bool TryCreateScript(GH_Component ghcomponent, string serialized, out object script)
    {
      script = default;

      if (s_projectServer is null) return false;

      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["component"] = ghcomponent;
      dctx.Inputs["project"] = s_project;
      dctx.Inputs["scriptData"] = serialized;

      if (s_projectServer.TryInvoke("plugins/v1/gh/deserialize", dctx))
      {
        return dctx.Outputs.TryGet("script", out script);
      }

      return false;
    }

    public static bool TryCreateScriptIcon(object script, out SD.Bitmap icon)
    {
      icon = default;

      if (s_projectServer is null) return false;

      dynamic ictx = ProjectInterop.CreateInvokeContext();
      ictx.Inputs["script"] = script;

      if (s_projectServer.TryInvoke("plugins/v1/icon/gh/script", ictx))
      {
        // server reports errors
        return ictx.Outputs.TryGet("icon", out icon);
      }

      return false;
    }

    public static void DisposeScript(GH_Component ghcomponent, object script)
    {
      if (script is null)
        return;

      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["component"] = ghcomponent;
      dctx.Inputs["project"] = s_project;
      dctx.Inputs["script"] = script;

      if (!s_projectServer.TryInvoke("plugins/v1/gh/dispose", dctx))
        throw new Exception("Error disposing Grasshopper script component");
    }
  }
}
