// This file is part of the SSMSJsonViewer project.
// See LICENSE file in the project root for license information.
//
// Copyright (c) Mister Magoo

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Task = System.Threading.Tasks.Task;

namespace SSMSJsonViewer
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[Guid(SSMSJsonViewerPackage.PackageGuidString)]
	public sealed class SSMSJsonViewerPackage : AsyncPackage, IDisposable
	{
		public const string PackageGuidString = "de3208e0-09db-4140-9b4c-a222739e9fa9";
		private XmlDocEventListener mySink;


		#region Package Members

		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			IVsRunningDocumentTable rdt = await GetServiceAsync(typeof(SVsRunningDocumentTable)).ConfigureAwait(true) as IVsRunningDocumentTable;
			if (rdt == null)
				return;
			mySink = new XmlDocEventListener();
			mySink.Initialise(rdt);
		}
		public void Dispose()
		{
			if (mySink == null)
				return;	
			((IDisposable)mySink).Dispose();
		}
		#endregion
	}
}
