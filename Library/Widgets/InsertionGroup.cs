﻿/*
Copyright (c) 2017, Kevin Pope, John Lewin
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatterHackers.DataConverters3D;
using MatterHackers.MatterControl.Library;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl.PrintLibrary
{
	public class InsertionGroup : Object3D
	{
		// TODO: Figure out how to collapse the InsertionGroup after the load task completes
		public InsertionGroup(IEnumerable<ILibraryItem> items)
		{
			Task.Run(async () =>
			{
				var newItemOffset = Vector2.Zero;

				// Filter to content file types only
				foreach (var item in items.Where(item => item.IsContentFileType()))
				{
					// Acquire
					var contentResult = item.CreateContent(null);
					if (contentResult != null)
					{
						// Add the placeholder
						var object3D = contentResult.Object3D;
						this.Children.Add(object3D);

						// Position at accumulating offset
						object3D.Matrix *= Matrix4X4.CreateTranslation(newItemOffset.x, newItemOffset.y, 0);

						// Wait for content to load
						await contentResult.MeshLoaded;

						// Adjust next item position
						// TODO: do something more interesting than increment in x
						newItemOffset.x += contentResult.Object3D.GetAxisAlignedBoundingBox(Matrix4X4.Identity).XSize;
					}
				}
			});
		}
	}
}
