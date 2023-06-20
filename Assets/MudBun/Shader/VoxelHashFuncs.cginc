/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_HASH_FUNCS
#define MUDBUN_VOXEL_HASH_FUNCS

#include "VoxelHashDefs.cginc"

#include "AabbTreeFuncs.cginc"
#include "BrushFuncs.cginc"
#include "Noise/RandomNoise.cginc"
#include "RenderModeDefs.cginc"
#include "VoxelFuncs.cginc"

#define kMaxVoxelHashCollisions (32) // (nodeHashTableSize)

// https://nosferalatu.com/SimpleGPUHashTable.html
int register_alloc_top_node(float3 center, int3 iCenter)
{
  float halfSize = 0.5f * currentNodeSize;

  uint key = top_node_key(iCenter);
  uint slot = key % nodeHashTableSize;

  int i = 0;
  while (i++ < kMaxVoxelHashCollisions)
  {
    uint prev = kNullVoxelHashKey;
    InterlockedCompareExchange(nodeHashTable[slot].key, kNullVoxelHashKey, key, prev);
    if (prev == kNullVoxelHashKey)
    {
      // newly registered
      int iNode = allocate_node(center, 0, -1, key);
      if (iNode < 0)
      {
        aNumNodesAllocated[0] = nodeHashTableSize;
        return -1;
      }

      int prevNumAllocated = 0;
      InterlockedAdd(aNumAllocation[kNumAllocationsVoxelHash], 1, prevNumAllocated);

      Aabb nodeAabb = make_aabb(center - halfSize, center + halfSize);
      nodePool[iNode].iBrushMask = allocate_node_brush_mask(iNode, nodeAabb);
      return iNode;
    }

    if (key == nodeHashTable[slot].key)
    {
      // already registered
      return -1; // already registered
    }
    else
    {
      // collision
      slot = (slot + 1) % nodeHashTableSize;
    }
  }
  return -1;
}

void register_brush_aabb(int iBrush)
{
  int op = aBrush[iBrush].op;
  float blend = aBrush[iBrush].blend;
  float maxDistFromCenter = currentNodeSize + blend;
  int iProxy = aBrush[iBrush].iProxy;
  Aabb aabb = aabbTree[iProxy].aabb;
  float3 clampedSurfaceShift = max(-aabb_extents(aabb), surfaceShift);
  aabb.boundsMin.xyz -= clampedSurfaceShift;
  aabb.boundsMax.xyz += clampedSurfaceShift;
  int3 iBoundsMin = int3(floor(aabb.boundsMin.xyz / currentNodeSize));
  int3 iBoundsMax = int3(floor(aabb.boundsMax.xyz / currentNodeSize));
  float halfNodeSize = 0.5f * currentNodeSize;
  for (int z = iBoundsMin.z; z <= iBoundsMax.z; ++z)
    for (int y = iBoundsMin.y; y <= iBoundsMax.y; ++y)
      for (int x = iBoundsMin.x; x <= iBoundsMax.x; ++x)
      {
        if (enable2dMode && z != 0)
          continue;

        float3 center = (float3(x, y, z) + 0.5f) * currentNodeSize;
        
        // profiler says it's faster if we don't do this extra brush evaluation
        /*
        float d = sdf_brush(kInfinity, center, aBrush[iBrush]);
        if (op == kSdfUnion)
        {
          if (abs(d) > maxDistFromCenter)
            continue;
        }
        else if (op == kSdfSubtract)
        {
          if (d > maxDistFromCenter)
            continue;
        }
        else if (op == kSdfIntersect)
        {
          if (-d > maxDistFromCenter)
            continue;
        }
        */

        register_alloc_top_node(center, int3(x, y, z));
      }
}

int register_alloc_child_node(float3 center, float size, int depth, int iParent, int idxAtDepth)
{
  float halfSize = 0.5f * size;
  Aabb aabb = make_aabb(center - halfSize, center + halfSize);

  uint key = nodePool[iParent].key;
  key = fnv_hash_concat(key, idxAtDepth);
  uint slot = key % nodeHashTableSize;

  int i = 0;
  while (i++ < kMaxVoxelHashCollisions)
  {
    uint prev = kNullVoxelHashKey;
    InterlockedCompareExchange(nodeHashTable[slot].key, kNullVoxelHashKey, key, prev);
    if (prev == kNullVoxelHashKey)
    {
      // newly registered
      int iNode = allocate_node(center, depth, iParent, key);
      if (iNode < 0)
      {
        aNumNodesAllocated[0] = nodeHashTableSize;
        return -1;
      }

      nodePool[iNode].iBrushMask = allocate_node_brush_mask(iNode, aabb);
      return iNode;
    }

    if (key == nodeHashTable[slot].key)
    {
      // already registered
      return -1;
    }
    else
    {
      // collision
      slot = (slot + 1) % nodeHashTableSize;
    }
  }
  return -1;
}

#endif

