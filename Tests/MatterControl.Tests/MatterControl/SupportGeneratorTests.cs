﻿/*
Copyright (c) 2016, Lars Brubaker, John Lewin
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
using System.Threading;
using System.Threading.Tasks;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.DataConverters3D;
using MatterHackers.MatterControl.DesignTools;
using MatterHackers.MatterControl.Tests.Automation;
using MatterHackers.PolygonMesh;
using MatterHackers.PolygonMesh.Processors;
using MatterHackers.VectorMath;
using NUnit.Framework;

namespace MatterControl.Tests.MatterControl
{
	[TestFixture]
	public class SupportGeneratorTests
	{
		[Test, Category("Support Generator")]
		public async Task SupportsFromBedTests()
		{
			var minimumSupportHeight = .05;

			// Set the static data to point to the directory of MatterControl
			AggContext.StaticData = new FileSystemStaticData(TestContext.CurrentContext.ResolveProjectPath(4, "StaticData"));
			MatterControlUtilities.OverrideAppDataLocation(TestContext.CurrentContext.ResolveProjectPath(4));

			// make a single cube in the air and ensure that support is generated
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			//
			// ______________
			{
				var scene = new InteractiveScene();

				var cube = await CubeObject3D.Create(20, 20, 20);
				var aabb = cube.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb.MinXYZ.Z + 15);
				scene.Children.Add(cube);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.Greater(scene.Children.Count, 1, "We should have added some support");
				foreach (var support in scene.Children.Where(i => i.OutputType == PrintOutputTypes.Support))
				{
					Assert.AreEqual(0, support.GetAxisAlignedBoundingBox().MinXYZ.Z, .001, "Support columns are all on the bed");
					Assert.AreEqual(15, support.GetAxisAlignedBoundingBox().ZSize, .02, "Support columns should be the right height from the bed");
				}
			}

			// make a single cube in the bed and ensure that no support is generated
			//   _________
			//   |       |
			// __|       |__
			//   |_______|
			{
				var scene = new InteractiveScene();

				var cube = await CubeObject3D.Create(20, 20, 20);
				var aabb = cube.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb.MinXYZ.Z - 5);
				scene.Children.Add(cube);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(1, scene.Children.Count, "We should not have added any support");
			}

			// make a cube on the bed and single cube in the air and ensure that support is not generated
			//    _________
			//    |       |
			//    |       |
			//    |_______|
			//    _________
			//    |       |
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z - 5);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 25);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// make a single cube in the bed and another cube on top, ensure that no support is generated
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			//   _________
			//   |       |
			// __|       |__
			//   |_______|
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 25);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// make a cube on the bed and another cube exactly on top of it and ensure that support is not generated
			//    _________
			//    |       |
			//    |       |
			//    |_______|
			//    |       |
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 20);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// make a cube on the bed and single cube in the air that intersects it and ensure that support is not generated
			//     _________
			//     |       |
			//     |______ |  // top cube actually exactly on top of bottom cube
			//    ||______||
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 15);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// Make a cube on the bed and single cube in the air that intersects it.
			// SELECT the cube on top
			// Ensure that support is not generated.
			//     _________
			//     |       |
			//     |______ |  // top cube actually exactly on top of bottom cube
			//    ||______||
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 15);
				scene.Children.Add(cubeInAir);

				scene.SelectedItem = cubeInAir;

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// Make a cube above the bed and a second above that. Ensure only one set of support material
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			// _______________
			{
				var scene = new InteractiveScene();

				var cube5AboveBed = await CubeObject3D.Create(20, 20, 20);
				var aabb5Above = cube5AboveBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube5AboveBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb5Above.MinXYZ.Z + 5);
				scene.Children.Add(cube5AboveBed);

				var cube30AboveBed = await CubeObject3D.Create(20, 20, 20);
				var aabb30Above = cube30AboveBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube30AboveBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb30Above.MinXYZ.Z + 30);
				scene.Children.Add(cube30AboveBed);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);

				Assert.Greater(scene.Children.Count, 1, "We should have added some support");
				foreach (var support in scene.Children.Where(i => i.OutputType == PrintOutputTypes.Support))
				{
					Assert.AreEqual(0, support.GetAxisAlignedBoundingBox().MinXYZ.Z, .001, "Support columns are all on the bed");
					Assert.AreEqual(5, support.GetAxisAlignedBoundingBox().ZSize, .02, "Support columns should be the right height from the bed");
				}
			}
		}

		[Test, Category("Support Generator")]
		public async Task SupportsEverywhereTests()
		{
			var minimumSupportHeight = .05;

			// Set the static data to point to the directory of MatterControl
			AggContext.StaticData = new FileSystemStaticData(TestContext.CurrentContext.ResolveProjectPath(4, "StaticData"));
			MatterControlUtilities.OverrideAppDataLocation(TestContext.CurrentContext.ResolveProjectPath(4));

			// make a single cube in the air and ensure that support is generated
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			//
			// ______________
			{
				var scene = new InteractiveScene();

				var cube = await CubeObject3D.Create(20, 20, 20);
				var aabb = cube.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb.MinXYZ.Z + 15);
				scene.Children.Add(cube);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.Normal
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.Greater(scene.Children.Count, 1, "We should have added some support");
				foreach (var support in scene.Children.Where(i => i.OutputType == PrintOutputTypes.Support))
				{
					Assert.AreEqual(0, support.GetAxisAlignedBoundingBox().MinXYZ.Z, .001, "Support columns are all on the bed");
					Assert.AreEqual(15, support.GetAxisAlignedBoundingBox().ZSize, .02, "Support columns should be the right height from the bed");
				}
			}

			// make a cube on the bed and single cube in the air and ensure that support is not generated
			//    _________
			//    |       |
			//    |       |
			//    |_______|
			//    _________
			//    |       |
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 25);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.Normal
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.Greater(scene.Children.Count, 2, "We should have added some support");
				foreach (var support in scene.Children.Where(i => i.OutputType == PrintOutputTypes.Support))
				{
					Assert.AreEqual(20, support.GetAxisAlignedBoundingBox().MinXYZ.Z, .001, "Support columns are all on the first cube");
					Assert.AreEqual(5, support.GetAxisAlignedBoundingBox().ZSize, .02, "Support columns should be the right height from the bed");
				}
			}

			// make a cube on the bed and single cube in the air that intersects it and ensure that support is not generated
			//     _________
			//     |       |
			//     |______ |  // top cube actually exactly on top of bottom cube
			//    ||______||
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 15);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.Normal
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// make a cube on the bed and another cube exactly on top of it and ensure that support is not generated
			//    _________
			//    |       |
			//    |       |
			//    |_______|
			//    |       |
			//    |       |
			// ___|_______|___
			{
				var scene = new InteractiveScene();

				var cubeOnBed = await CubeObject3D.Create(20, 20, 20);
				var aabbBed = cubeOnBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeOnBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbBed.MinXYZ.Z);
				scene.Children.Add(cubeOnBed);

				var cubeInAir = await CubeObject3D.Create(20, 20, 20);
				var aabbAir = cubeInAir.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cubeInAir.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbAir.MinXYZ.Z + 20);
				scene.Children.Add(cubeInAir);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.From_Bed
				};
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(2, scene.Children.Count, "We should not have added support");
			}

			// Make a cube above the bed and a second above that. Ensure only one set of support material
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			//   _________
			//   |       |
			//   |       |
			//   |_______|
			// _______________
			{
				var scene = new InteractiveScene();

				var cube5AboveBed = await CubeObject3D.Create(20, 20, 20);
				var aabb5Above = cube5AboveBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube5AboveBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb5Above.MinXYZ.Z + 5);
				scene.Children.Add(cube5AboveBed);

				var cube30AboveBed = await CubeObject3D.Create(20, 20, 20);
				var aabb30Above = cube30AboveBed.GetAxisAlignedBoundingBox();
				// move it so the bottom is 15 above the bed
				cube30AboveBed.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabb30Above.MinXYZ.Z + 30);
				scene.Children.Add(cube30AboveBed);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight)
				{
					SupportType = SupportGenerator.SupportGenerationType.Normal
				};
				await supportGenerator.Create(null, CancellationToken.None);

				Assert.Greater(scene.Children.Count, 2, "We should have added some support");
				var bedSupportCount = 0;
				var airSupportCount = 0;
				foreach (var support in scene.Children.Where(i => i.OutputType == PrintOutputTypes.Support))
				{
					var aabb = support.GetAxisAlignedBoundingBox();
					Assert.AreEqual(5, aabb.ZSize, .001, "Support columns should be the right height from the bed");
					if (aabb.MinXYZ.Z > -.001 && aabb.MinXYZ.Z < .001) // it is on the bed
					{
						// keep track of the count
						bedSupportCount++;
					}
					else
					{
						airSupportCount++;
						// make sure it is the right height
						Assert.AreEqual(25, aabb.MinXYZ.Z, .001, "Support columns are all on the bed");
					}
				}

				Assert.AreEqual(bedSupportCount, airSupportCount, "Same number of support columns in each space.");
			}

			// load a complex part that should have no support required
			if(false)
			{
				InteractiveScene scene = new InteractiveScene();

				var meshPath = TestContext.CurrentContext.ResolveProjectPath(4, "Tests", "TestData", "TestParts", "NoSupportNeeded.stl");

				var supportObject = new Object3D()
				{
					Mesh = StlProcessing.Load(meshPath, CancellationToken.None)
				};

				var aabbCube = supportObject.GetAxisAlignedBoundingBox();
				// move it so the bottom is on the bed
				supportObject.Matrix = Matrix4X4.CreateTranslation(0, 0, -aabbCube.MinXYZ.Z);
				scene.Children.Add(supportObject);

				var supportGenerator = new SupportGenerator(scene, minimumSupportHeight);
				supportGenerator.SupportType = SupportGenerator.SupportGenerationType.Normal;
				await supportGenerator.Create(null, CancellationToken.None);
				Assert.AreEqual(1, scene.Children.Count, "We should not have added support");
			}
		}

		[Test, Category("Support Generator")]
		public void TopBottomWalkingTest()
		{
			// a box in the air
			{
				var planes = new SupportGenerator.HitPlanes(0)
				{
					new SupportGenerator.HitPlane(0, false),  // top at 0 (the bed)
					new SupportGenerator.HitPlane(5, true),   // bottom at 5 (the bottom of a box)
					new SupportGenerator.HitPlane(10, false), // top at 10 (the top of the box)
				};

				int bottom = planes.GetNextBottom(0);
				Assert.AreEqual(1, bottom); // we get the bottom

				int bottom1 = planes.GetNextBottom(1);
				Assert.AreEqual(-1, bottom1, "There are no more bottoms so we get back a -1.");
			}

			// two boxes, the bottom touching the bed, the top touching the bottom
			{
				var planes = new SupportGenerator.HitPlanes(0)
				{
					new SupportGenerator.HitPlane(0, false),  // top at 0 (the bed)
					new SupportGenerator.HitPlane(0, true),  // bottom at 0 (box a on bed)
					new SupportGenerator.HitPlane(10, false), // top at 10 (box a top)
					new SupportGenerator.HitPlane(10, true), // bottom at 10 (box b bottom)
					new SupportGenerator.HitPlane(20, false) // top at 20 (box b top)
				};

				int bottom = planes.GetNextBottom(0);
				Assert.AreEqual(-1, bottom, "The boxes are sitting on the bed and no support is required");
			}

			// two boxes, the bottom touching the bed, the top inside the bottom
			{
				var planes = new SupportGenerator.HitPlanes(0)
				{
					new SupportGenerator.HitPlane(0, false),  // top at 0 (the bed)
					new SupportGenerator.HitPlane(0, true),  // bottom at 0 (box a on bed)
					new SupportGenerator.HitPlane(5, true), // bottom at 5 (box b bottom)
					new SupportGenerator.HitPlane(10, false), // top at 10 (box a top)
					new SupportGenerator.HitPlane(20, false) // top at 20 (box b top)
				};

				int bottom = planes.GetNextBottom(0);
				Assert.AreEqual(-1, bottom, "The boxes are sitting on the bed and no support is required");
			}

			// get next top skips any tops before checking for bottom
			{
				var planes = new SupportGenerator.HitPlanes(0)
				{
					new SupportGenerator.HitPlane(0, false),
					new SupportGenerator.HitPlane(5, true),
					new SupportGenerator.HitPlane(10, false),
					new SupportGenerator.HitPlane(20, false),
					new SupportGenerator.HitPlane(25, true)
				};

				int top = planes.GetNextTop(0);
				Assert.AreEqual(3, top);
			}

			// actual output from a dual extrusion print that should have no support
			{
				var planes = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, false),
					new SupportGenerator.HitPlane(0.0302, true),
					new SupportGenerator.HitPlane(0.0497, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(15.95, false),
					new SupportGenerator.HitPlane(15.9697, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
				};

				int bottom = planes.GetNextBottom(0);
				Assert.AreEqual(-1, bottom, "The boxes are sitting on the bed and no support is required");
			}

			// simplify working as expected (planes with space turns into tow start end sets)
			{
				var planes = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(0, false),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, false),
					new SupportGenerator.HitPlane(0.0302, true),
					new SupportGenerator.HitPlane(0.0497, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(15.95, false),
					new SupportGenerator.HitPlane(15.9697, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(20, true),
					new SupportGenerator.HitPlane(25, false),
				};

				planes.Simplify();
				Assert.AreEqual(4, planes.Count, "After simplify there should be two ranges");
			}

			// pile of plats turns into 1 start end
			{
				var planes = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(0, false),
					new SupportGenerator.HitPlane(0.0302, true),
					new SupportGenerator.HitPlane(0.0497, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, true),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(0.762, false),
					new SupportGenerator.HitPlane(15.95, false),
					new SupportGenerator.HitPlane(15.9697, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(16, false),
				};

				planes.Simplify();
				Assert.AreEqual(2, planes.Count, "After simplify there should one range");
				Assert.IsTrue(planes[0].Bottom, "Is Bottom");
				Assert.IsFalse(planes[1].Bottom, "Is Top");
			}

			// merge of two overlapping sets tuns into one set
			{
				var planes0 = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(16, false),
					new SupportGenerator.HitPlane(20, true),
					new SupportGenerator.HitPlane(25, false),
				};

				var planes1 = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(16, true),
					new SupportGenerator.HitPlane(20, false),
				};

				planes0.Merge(planes1);
				Assert.AreEqual(2, planes0.Count, "After merge there should one range");
				Assert.AreEqual(0, planes0[0].Z);
				Assert.IsTrue(planes0[0].Bottom);
				Assert.AreEqual(25, planes0[1].Z);
				Assert.IsFalse(planes0[1].Bottom);
			}

			// merge of two non-overlapping sets stays two sets
			{
				var planes0 = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(16, false),
				};

				var planes1 = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(17, true),
					new SupportGenerator.HitPlane(20, false),
				};

				planes0.Merge(planes1);
				Assert.AreEqual(4, planes0.Count, "After merge there should be two ranges");
				Assert.AreEqual(0, planes0[0].Z);
				Assert.IsTrue(planes0[0].Bottom);
				Assert.AreEqual(16, planes0[1].Z);
				Assert.IsFalse(planes0[1].Bottom);
				Assert.AreEqual(17, planes0[2].Z);
				Assert.IsTrue(planes0[2].Bottom);
				Assert.AreEqual(20, planes0[3].Z);
				Assert.IsFalse(planes0[3].Bottom);
			}

			// merge of two overlapping sets (within tolerance turns into one set
			{
				var planes0 = new SupportGenerator.HitPlanes(5)
				{
					new SupportGenerator.HitPlane(0, true),
					new SupportGenerator.HitPlane(16, false),
				};

				var planes1 = new SupportGenerator.HitPlanes(.1)
				{
					new SupportGenerator.HitPlane(17, true),
					new SupportGenerator.HitPlane(20, false),
				};

				planes0.Merge(planes1);
				Assert.AreEqual(2, planes0.Count, "After merge there should one range");
				Assert.AreEqual(0, planes0[0].Z, "Starts at 0");
				Assert.IsTrue(planes0[0].Bottom, "Is Bottom");
				Assert.AreEqual(20, planes0[1].Z, "Goes to 20");
				Assert.IsFalse(planes0[1].Bottom, "Is Top");
			}
		}
	}
}
