/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_FUNCS
#define MUDBUN_VOXEL_FUNCS

#include "VoxelDefs.cginc"

int4 get_voxel_tree_branching_factors()
{
  uint4 factors;
  unpack_8888(uint(voxelTreeBranchingFactorsCompressed), factors.x, factors.y, factors.z, factors.w);
  return int4(factors);
}

int allocate_node(float3 center, int nodeDepth, int iParent, int key)
{
  int iNode;
  InterlockedAdd(aNumNodesAllocated[nodeDepth + 1], 1, iNode);
  InterlockedAdd(aNumNodesAllocated[0], 1, iNode);

  if (iNode < int(nodePoolSize))
  {
    nodePool[iNode].center = center;
    nodePool[iNode].iParent = iParent;
    nodePool[iNode].iBrushMask = -1;
    nodePool[iNode].key = key;
  }
  else
  {
    aNumNodesAllocated[0] = nodePoolSize;
    iNode = -1;
  }

  return iNode;
}

#endif

