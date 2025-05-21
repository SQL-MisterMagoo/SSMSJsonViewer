// This file is part of the SSMSJsonViewer project.
// See LICENSE file in the project root for license information.
//
// Copyright (c) Mister Magoo
//

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

public class XmlDocEventListener : IVsRunningDocTableEvents,IDisposable
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
		if (runningDocumentTable is null)
			return VSConstants.S_OK;

		IntPtr docData = IntPtr.Zero;
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

			// Get the text buffer
			var dataObject = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(docData);
			var textLines = dataObject as Microsoft.VisualStudio.TextManager.Interop.IVsTextLines;
			if (textLines == null)
				return VSConstants.S_OK;

			// Get the text
			textLines.GetLastLineIndex(out int lastLine, out int lastIndex);
			textLines.GetLineText(0, 0, lastLine, lastIndex, out string text);

			// Try to parse as JSON - if it errors out, do nothing
			try
			{
				var parsed = Newtonsoft.Json.Linq.JToken.Parse(text);
				var formatted = parsed.ToString(Newtonsoft.Json.Formatting.Indented);

				IntPtr formattedPtr = System.Runtime.InteropServices.Marshal.StringToBSTR(formatted);
				try
				{
					textLines.ReplaceLines(0, 0, lastLine, lastIndex, formattedPtr, formatted.Length, null);
				}
				finally
				{
					if (formattedPtr != IntPtr.Zero)
						System.Runtime.InteropServices.Marshal.FreeBSTR(formattedPtr);
				}
			}
			catch (Newtonsoft.Json.JsonReaderException)
			{
				// Not valid JSON, do nothing
			}
		}
		finally
		{
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