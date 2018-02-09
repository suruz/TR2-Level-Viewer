using UnityEngine;

public class Triangle  
{
	public Vertex[] vertex; 
	public  Vector3 normal;
	public Triangle(Vertex v0,Vertex v1,Vertex v2)
	{
		vertex = new Vertex[3];
		vertex[0] = v0;
		vertex[1] = v1;
		vertex[2] = v2;
		
		vertex[0].AddNeighbor(v1);
		vertex[0].AddNeighbor(v2);
		
		vertex[1]. AddNeighbor(v0);
		vertex[1]. AddNeighbor(v2);
		
		vertex[2]. AddNeighbor(v0);
		vertex[2]. AddNeighbor(v1);
		
		ComputeNormal();
	}
	
	public void deTriangle ( )
	{
    	vertex[0].RemoveFace( this );
    	vertex[1].RemoveFace( this );
   		vertex[2].RemoveFace( this );
    
    	// remove from neighbor
   	 	vertex[0].RemoveIfNonNeighbor( vertex[1] );
    	vertex[0].RemoveIfNonNeighbor( vertex[2] );
   		vertex[1].RemoveIfNonNeighbor( vertex[0] );
    	vertex[1].RemoveIfNonNeighbor( vertex[2] );
    	vertex[2].RemoveIfNonNeighbor( vertex[1]);
   		vertex[2].RemoveIfNonNeighbor( vertex[0]);
        
    	//deleted = true;
   		//his.RemovedTriangle( id );
	}
	
	public void ComputeNormal()
	{
		Vector3 vec0 = vertex[0].position;
		Vector3 vec1 = vertex[1].position;
		Vector3 vec2 = vertex[2].position;
		normal= (Vector3.Cross(vec1 - vec0, vec2 - vec1)).normalized;

		/*if(Vector3.Dot(normal, Vector3.up ) > 0.5f)
		{
			normal = Vector3.up;
		}*/

	}
	
	//this face is one of the srrounding faces attahed to vold
	public void ReplaceVertex(Vertex vold,Vertex vnew) 
	{
		for(int i = 0; i < 3; i++)
		{
			
			if(vertex[i].position == vold.position  )
			{
				vertex[i]  = vnew;
				vold.RemoveFace( this );
    			
				if(vnew!=null)
				vnew.AddFace( this );
				break;
			}
		}
		
		//remove reff of vold from Neighborhood
				vold.RemoveIfNonNeighbor( vertex[0] );
    			vertex[0].RemoveIfNonNeighbor( vold );
   				vold.RemoveIfNonNeighbor( vertex[1] );
   				vertex[1].RemoveIfNonNeighbor( vold );
				vold.RemoveIfNonNeighbor( 	vertex[2] );
    			vertex[2].RemoveIfNonNeighbor( vold );
				
				
				vertex[0]. AddNeighbor(vertex[1]);
				vertex[0]. AddNeighbor(vertex[2]);
		
				vertex[1]. AddNeighbor(vertex[0]);
				vertex[1]. AddNeighbor(vertex[2]);
		
				vertex[2]. AddNeighbor(vertex[0]);
				vertex[2]. AddNeighbor(vertex[1]);
				ComputeNormal();
	}
	
	public bool HasVertex(Vertex v)
	{
		bool retval = false;
		
		for(int i = 0; i < vertex.Length; i++)
		{
			
			if(vertex[i].position == v.position  )
			{
				retval = true;
				break;
			}
		}
		
		return retval;
	}
}


