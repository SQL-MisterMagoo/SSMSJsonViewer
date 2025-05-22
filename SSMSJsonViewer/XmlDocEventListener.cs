// This file is part of the SSMSJsonViewer project.
// See LICENSE file in the project root for license information.
//
// Copyright (c) Mister Magoo
//

using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

public class XmlDocEventListener : IVsRunningDocTableEvents, IDisposable
{
  private IVsRunningDocumentTable runningDocumentTable;
  private uint pdwCookie;

  public void Initialise(IVsRunningDocumentTable rdt)
  {
    runningDocumentTable = rdt;
    if (runningDocumentTable != null)
    {
      runningDocumentTable.AdviseRunningDocTableEvents(this, out uint pdwCookie);
      this.pdwCookie = pdwCookie;
    }
  }
  public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
  {
    Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

    if (runningDocumentTable is null)
      return VSConstants.S_OK;

    IntPtr docData = IntPtr.Zero;
    object dataObject = null;
    Microsoft.VisualStudio.TextManager.Interop.IVsTextLines textLines = null;

    try
    {
      int hr = runningDocumentTable.GetDocumentInfo(
        docCookie,
        out _, // flags
        out _, //readLocks
        out _, //editLocks
        out string moniker,
        out _, //hierarchy
        out _, //itemidOut
        out docData
      );
      if (hr != VSConstants.S_OK || string.IsNullOrWhiteSpace(moniker) || !moniker.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        return VSConstants.S_OK;

      dataObject = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(docData);
      textLines = dataObject as Microsoft.VisualStudio.TextManager.Interop.IVsTextLines;
      if (textLines == null)
        return VSConstants.S_OK;

      textLines.GetLastLineIndex(out int lastLine, out int lastIndex);
      textLines.GetLineText(0, 0, lastLine, lastIndex, out string text);

      try
      {
        var parsed = Newtonsoft.Json.Linq.JToken.Parse(text);
        var formatted = parsed.ToString(Newtonsoft.Json.Formatting.Indented);

        var dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
        if (dte == null)
          return VSConstants.S_OK;

        var window = dte.ItemOperations.NewFile("General\\Text File", Path.ChangeExtension(Path.GetFileNameWithoutExtension(moniker), ".json"));
        if (window == null)
          return VSConstants.S_OK;

        var textDoc = window.Document?.Object("TextDocument") as EnvDTE.TextDocument;
        if (textDoc != null)
        {
          var editPoint = textDoc.StartPoint.CreateEditPoint();
          editPoint.Insert(formatted);
        }

        foreach (EnvDTE.Document document in dte.Documents)
        {
          if (document.FullName.Equals(moniker, StringComparison.OrdinalIgnoreCase))
          {
            document.Close(EnvDTE.vsSaveChanges.vsSaveChangesNo);
            break;
          }
        }
      }
      catch (Newtonsoft.Json.JsonReaderException)
      {
        // Not valid JSON, do nothing
      }
      catch (Exception ex)
      {
        // Log or handle unexpected exceptions
        System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex}");
      }
    }
    finally
    {
      if (textLines != null)
        System.Runtime.InteropServices.Marshal.ReleaseComObject(textLines);
      if (dataObject != null)
        System.Runtime.InteropServices.Marshal.ReleaseComObject(dataObject);
      if (docData != IntPtr.Zero)
        System.Runtime.InteropServices.Marshal.Release(docData);
    }
    return VSConstants.S_OK;
  }
  public int OnAfterSave(uint docCookie) => VSConstants.S_OK;
  public int OnBeforeSave(uint docCookie) => VSConstants.S_OK;
  public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;
  public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;
  public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;
  public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;
  public void Dispose()
  {
    if (runningDocumentTable != null && pdwCookie != 0)
    {
      runningDocumentTable.UnadviseRunningDocTableEvents(pdwCookie);
    }
  }
}