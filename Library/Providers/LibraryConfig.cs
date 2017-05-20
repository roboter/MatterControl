﻿/*
Copyright (c) 2017, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using MatterHackers.Agg.PlatformAbstract;

namespace MatterHackers.MatterControl.Library
{
	public interface ILibraryContext
	{
		ILibraryContainer ActiveContainer { get; set; }
		event EventHandler<ContainerChangedEventArgs> ContainerChanged;
		event EventHandler<ContainerChangedEventArgs> ContainerReloaded;
	}

	public class ContainerChangedEventArgs : EventArgs
	{
		public ContainerChangedEventArgs(ILibraryContainer activeContainer, ILibraryContainer previousContainer)
		{
			this.ActiveContainer = activeContainer;
			this.PreviousContainer = previousContainer;
		}

		public ILibraryContainer ActiveContainer { get; }
		public ILibraryContainer PreviousContainer { get; }
	}

	public class LibraryConfig : ILibraryContext
	{
		public event EventHandler<ContainerChangedEventArgs> ContainerChanged;
		public event EventHandler<ContainerChangedEventArgs> ContainerReloaded;

		// TODO: Needed?
		public event EventHandler LibraryItemsChanged;

		/// <summary>
		/// Storage for plugin 'Creators' added via RegisterCreator
		/// </summary>
		private CreatorsContainer registeredCreators;

		private List<ILibraryContainerLink> libraryProviders;

		private ILibraryContainer activeContainer;
		
		public LibraryConfig()
		{
			registeredCreators = new CreatorsContainer()
			{
				Name = "Creators",
				ChildContainers = new List<ILibraryContainerLink>(),
				Items = new List<ILibraryItem>()
			};

			libraryProviders = new List<ILibraryContainerLink>()
			{
				new DynamicContainerLink(
					registeredCreators.Name,
					StaticData.Instance.LoadIcon("icon_creator.png"),
					() => registeredCreators),
			};

			this.RootLibaryContainer = new RootLibraryContainer(libraryProviders);

			this.ActiveContainer = this.RootLibaryContainer;
		}

		public ILibraryContainer RootLibaryContainer { get; }

		public Dictionary<string, IContentProvider> ContentProviders = new Dictionary<string, IContentProvider>();

		public ILibraryContainer ActiveContainer
		{
			get
			{
				return activeContainer;
			}
			set
			{
				if (activeContainer == value)
				{
					return;
				}

				var newContainer = value;

				var eventArgs = new ContainerChangedEventArgs(newContainer, activeContainer);

				if (activeContainer != null)
				{
					activeContainer.Deactivate();
					activeContainer.Reloaded -= ActiveContainer_Reloaded;
					activeContainer.KeywordFilter = "";

					// If the new container is an ancestor of the active container we need to Dispose everyone up to that point
					if (activeContainer.Parents().Where(p => p == newContainer).Any())
					{
						var context = activeContainer;
						while (context != newContainer)
						{
							context.Dispose();
							context = context.Parent;
						}
					}
				}

				activeContainer = newContainer;
				activeContainer.Activate();
				activeContainer.Reloaded += ActiveContainer_Reloaded;

				ContainerChanged?.Invoke(this, eventArgs);
			}
		}

		public IContentProvider GetContentProvider(ILibraryItem item)
		{
			string contentType = (item as ILibraryContentStream)?.ContentType ?? (item as ILibraryContentItem)?.ContentType;
			if (contentType == null)
			{
				return null;
			}

			return GetContentProvider(contentType);
		}

		public IContentProvider GetContentProvider(string contentType)
		{
			IContentProvider provider;
			ContentProviders.TryGetValue(contentType, out provider);

			return provider;
		}

		public void RegisterRootProvider(ILibraryContainerLink containerItem)
		{
			libraryProviders.Add(containerItem);
			OnLibraryItemsChanged();
		}

		public void RegisterCreator(ILibraryContainerLink containerItem)
		{
			registeredCreators.ChildContainers.Add(containerItem);
			OnLibraryItemsChanged();
		}

		protected void OnLibraryItemsChanged()
		{
			LibraryItemsChanged?.Invoke(this, null);
		}

		private void ActiveContainer_Reloaded(object sender, EventArgs args)
		{
			ContainerReloaded?.Invoke(this, new ContainerChangedEventArgs(this.ActiveContainer, null));
		}
	}
}
