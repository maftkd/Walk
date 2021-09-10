using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMan : MonoBehaviour
{
	public Material _terrainMat;
	
	//test values
	public int _squareLength;//=25;
	public float _divsPerUnit;// = 4;
	public int _octaves;//=1;
	public float _offset;
	public float _scale;//=10f;
	[Tooltip("amplitude multiplier")]
	public float _persistence;
	[Tooltip("frequency multiplier")]
	public float _lacunarity;
	public float _baseFrequency;
	public float _baseAmplitude;

    // Start is called before the first frame update
    void Start()
    {
		GenTestTerrain();
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.T))
			GenTestTerrain();
    }

	void GenTestTerrain(){
		if(GameObject.Find("Test Terrain")!=null)
			GameObject.Destroy(GameObject.Find("Test Terrain"));

		//simplex noise
		SimplexNoiseGenerator noise = new SimplexNoiseGenerator();

		//create go and components
		GameObject testGo = new GameObject("Test Terrain");
		MeshFilter meshF = testGo.AddComponent<MeshFilter>();
		MeshRenderer meshR = testGo.AddComponent<MeshRenderer>();
		MeshCollider meshC = testGo.AddComponent<MeshCollider>();

		//allocate some data
		int numVertCols=Mathf.RoundToInt(_squareLength*_divsPerUnit)+1;
		int numVertRows=numVertCols;
		int numVerts=numVertCols*numVertRows;
		int numTriIndices=(numVertCols-1)*(numVertRows-1)*2*3;
		Mesh m = new Mesh();
		Vector3[] verts = new Vector3[numVerts];
		Vector2[] uvs = new Vector2[numVerts];
		Vector3[] normals = new Vector3[numVerts];
		int[] triangleIndices = new int[numTriIndices];

		//get position / uv
		float xStart=-_squareLength/2.0f;
		float zStart=-_squareLength/2.0f;
		float vertSpacing=1.0f/_divsPerUnit;
		int vIndex=0;
		int triIndex=0;
		float xPos=0;float zPos=0; float yPos=0;
		float totalAmp=0;
		float frequency=1;
		float amplitude=1;
		for(int z=0; z<numVertRows; z++){
			for(int x=0; x<numVertCols; x++){
				vIndex=z*numVertCols+x;
				//get pos
				xPos=xStart+x*vertSpacing;
				zPos=zStart+z*vertSpacing;
				yPos=0;
				totalAmp=0;
				frequency=_baseFrequency;
				amplitude=_baseAmplitude;
				for(int o=0; o<_octaves; o++){
					yPos+=noise.coherentNoise((transform.position.x+xPos)*frequency,0,
							(transform.position.z+zPos)*frequency)*amplitude;
					//totalAmp+=amplitude;
					amplitude*=_persistence;
					frequency*=_lacunarity;
				}
				yPos=yPos*_scale+_offset;
				//yPos=(yPos/totalAmp)*_scale+_offset;

				/*
				for(int o=1; o<=octaves; o++){
					yPos+=noise.coherentNoise((transform.position.x+xPos)*Mathf.Pow(pFreq,o*2),0,
							(transform.position.z+zPos)*Mathf.Pow(pFreq,o*2))*(pAmp/(o*2));
				}
				*/

				verts[vIndex]=new Vector3(xPos,yPos,zPos);
				//get uv
				uvs[vIndex]=new Vector2(x/(float)(numVertCols-1),z/(float)(numVertRows-1));
				//get triangle indices
				if(z<numVertRows-1 && x<numVertCols-1){
					triangleIndices[triIndex]=vIndex;
					triangleIndices[triIndex+1]=vIndex+numVertCols;
					triangleIndices[triIndex+2]=vIndex+1;
					triangleIndices[triIndex+3]=vIndex+1;
					triangleIndices[triIndex+4]=vIndex+numVertCols;
					triangleIndices[triIndex+5]=vIndex+numVertCols+1;
					triIndex+=6;
				}
			}
		}

		//calculate norms
		Vector3 xDir=Vector3.zero;
		Vector3 zDir=Vector3.zero;
		for(int z=0; z<numVertRows; z++){
			for(int x=0; x<numVertCols; x++){
				vIndex=z*numVertCols+x;
				//get dx
				if(x==0)
					xDir=verts[vIndex+1]-verts[vIndex];
				else if(x==numVertCols-1)
					xDir=verts[vIndex]-verts[vIndex-1];
				else
					xDir=verts[vIndex+1]-verts[vIndex-1];
				//get dz
				if(z==0)
					zDir=verts[vIndex+numVertCols]-verts[vIndex];
				else if(z==numVertRows-1)
					zDir=verts[vIndex]-verts[vIndex-numVertCols];
				else
					zDir=verts[vIndex+numVertCols]-verts[vIndex-numVertCols];
				xDir.Normalize();
				zDir.Normalize();
				normals[vIndex]=Vector3.Cross(zDir,xDir);
			}
		}
		m.vertices=verts;
		m.uv=uvs;
		m.normals=normals;
		m.triangles=triangleIndices;

		meshF.mesh=m;
		meshC.sharedMesh=m;
		meshR.material=_terrainMat;
	}
}
