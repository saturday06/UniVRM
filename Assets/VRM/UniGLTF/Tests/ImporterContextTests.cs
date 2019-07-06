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
                using (var exporter = new gltfExporter(gltf))
                {
                    exporter.Prepare(go);
                    exporter.Export();
                }

                var context = new ImporterContext();
                context.ParseGlb(gltf.ToGlbBytes());
                context.Load();
                context.ShowMeshes();
                context.EnableUpdateWhenOffscreen();

                var importedCubeA = context.Root.transform.GetChild(0);
                Assert.AreEqual("red", importedCubeA.GetComponent<Renderer>().sharedMaterial.name);
                Assert.AreEqual(Color.red, importedCubeA.GetComponent<Renderer>().sharedMaterial.color);

                var importedCubeB = context.Root.transform.GetChild(1);

                Assert.AreEqual(Color.red, importedCubeB.GetComponent<Renderer>().sharedMaterial.color);
                Assert.AreEqual("red", importedCubeB.GetComponent<Renderer>().sharedMaterial.name);

                importedCubeA.GetComponent<Renderer>().sharedMaterial.name = "green";
                importedCubeA.GetComponent<Renderer>().sharedMaterial.color = Color.green;

                Assert.AreEqual(Color.green, importedCubeB.GetComponent<Renderer>().sharedMaterial.color);
                Assert.AreEqual("green", importedCubeB.GetComponent<Renderer>().sharedMaterial.name);

                Assert.AreEqual(Color.green, importedCubeB.GetComponent<Renderer>().material.color);
                Assert.AreEqual("green (Instance)", importedCubeB.GetComponent<Renderer>().material.name);
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }
    }
}
