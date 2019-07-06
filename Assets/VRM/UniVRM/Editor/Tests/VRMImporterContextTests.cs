using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace VRM
{
    public class VRMImporterContextTests
    {
        [Test]
        public void SameMaterialButDifferentMeshImport()
        {
            var go = new GameObject("same_material");
            try
            {
                var shader = Shader.Find("Unlit/Color");
                var material = new Material(shader);
                material.name = "red";
                material.color = Color.red;

                var cubeA = GameObject.CreatePrimitive(PrimitiveType.Cube);
                {
                    cubeA.transform.SetParent(go.transform);
                    cubeA.GetComponent<Renderer>().sharedMaterial = material;
                }

                var cubeB = GameObject.Instantiate(cubeA);
                {
                    cubeB.transform.SetParent(go.transform);
                    cubeB.GetComponent<Renderer>().sharedMaterial = material;
                }

                // export
                var gltf = new glTF();
                using (var exporter = new VRMExporter(gltf))
                {
                    exporter.Prepare(go);
                    exporter.Export();
                }

                var context = new VRMImporterContext();
                context.ParseGlb(gltf.ToGlbBytes());
                context.Load();
                context.ShowMeshes();
                context.EnableUpdateWhenOffscreen();

                var importedCubeA = context.Root.transform.GetChild(0);
                var importedCubeAMaterial = importedCubeA.GetComponent<Renderer>().sharedMaterial;
                Assert.AreEqual("red", importedCubeAMaterial.name);
                Assert.AreEqual(Color.red, importedCubeAMaterial.color);

                var importedCubeB = context.Root.transform.GetChild(1);
                var importedCubeBMaterial = importedCubeB.GetComponent<Renderer>().sharedMaterial;
                Assert.AreEqual("red", importedCubeBMaterial.name);
                Assert.AreEqual(Color.red, importedCubeBMaterial.color);

                importedCubeAMaterial.name = "modified";
                Assert.AreEqual("modified", importedCubeBMaterial.name);
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }
    }
}
