// references
// https://twitter.com/Cyanilux/status/1396848736022802435
// https://roystan.net/articles/grass-shader
// https://github.com/ColinLeung-NiloCat/UnityURP-MobileDrawMeshInstancedIndirectExample
// https://www.patreon.com/posts/24192529
// https://drive.google.com/file/d/1E_vjNknY1w4dnMadBQsZfh86-J4jd5O0/view
// https://youtu.be/qcScJ_vgsGU
// https://youtu.be/ecYWvfMoRIM

using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace MuddleClub.MuddleMages.Grass
{
    public class Grass : MonoBehaviour
    {
        private const bool ReceiveShadows = true;
        private static readonly int PropertiesBufferID = Shader.PropertyToID("properties");

        public int count;
        public float range; // (terrain width)

        public Terrain terrain;

        public Mesh mesh;
        public Material material;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.TwoSided;
        private ComputeBuffer _argsBuffer;

        private Bounds _bounds;
        private ComputeBuffer _instancesBuffer;

        private Matrix4x4[] _matrices;
        private MaterialPropertyBlock _mpb;

        public void Update()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, _bounds, _argsBuffer, 0, _mpb, shadowCasting);
        }

        public void OnEnable()
        {
            _mpb = new MaterialPropertyBlock();
            _bounds = new Bounds(terrain.transform.position, new Vector3(200, 200, 200));
            InitializeBuffers();
        }

        private void OnDisable()
        {
            if (_instancesBuffer != null)
            {
                _instancesBuffer.Release();
                _instancesBuffer = null;
            }

            if (_argsBuffer != null)
            {
                _argsBuffer.Release();
                _argsBuffer = null;
            }
        }

        private void InitializeBuffers()
        {
            // Args
            var args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)count;
            args[2] = mesh.GetIndexStart(0);
            args[3] = mesh.GetBaseVertex(0);
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);

            // Instances
            var instances = new MeshProperties[count];
            for (var i = 0; i < count; i++)
            {
                var data = new MeshProperties();

                //Vector3 position = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                var terrainPos = terrain.transform.position;
                var position = terrainPos + new Vector3(Random.Range(0, range), 0, Random.Range(0, range));

                var y = terrain.SampleHeight(position);
                position.y += y;

                var terrainData = terrain.terrainData;
                var normal = terrain.terrainData.GetInterpolatedNormal(
                    (position.x - terrainPos.x) / terrainData.size.x,
                    (position.z - terrainPos.z) / terrainData.size.z
                );

                var proj = Vector3.up - Vector3.Dot(Vector3.up, normal) * normal;
                var rotation = Quaternion.LookRotation(proj, normal);
                rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), normal) * rotation;
                rotation.Normalize();
                data.trs = Matrix4x4.TRS(position, rotation, Vector3.one);
                instances[i] = data;
            }

            _instancesBuffer = new ComputeBuffer(count, MeshProperties.Size());
            _instancesBuffer.SetData(instances);
            material.SetBuffer(PropertiesBufferID, _instancesBuffer);
        }

        private struct MeshProperties
        {
            public Matrix4x4 trs;

            public static int Size()
            {
                unsafe
                {
                    return sizeof(MeshProperties);
                }
            }
        }
    }
}
