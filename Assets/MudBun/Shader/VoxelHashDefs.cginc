/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_HASH_DEFS
#define MUDBUN_VOXEL_HASH_DEFS

#define kNullVoxelHashKey (0)

struct VoxelHashEntry
{
  uint key;
  int iGenPoint;
};

VoxelHashEntry init_voxel_hash_entry()
{
  VoxelHashEntry entry;
  entry.key = kNullVoxelHashKey;
  entry.iGenPoint = -1;
  return entry;
}

uint top_node_key(int3 iCenter)
{
  /*
  iCenter = clamp(iCenter + 512, int3(0, 0, 0), int3(1023, 1023, 1023));
  return (uint(iCenter.x) << 21) | (uint(iCenter.y) << 11) | (uint(iCenter.z) << 1) | 1;
  */

  uint hash = fnv_hash_concat(kFnvDefaultBasis, uint(iCenter.x));
  hash = fnv_hash_concat(hash, uint(iCenter.y));
  hash = fnv_hash_concat(hash, uint(iCenter.z));
  return hash;
}

RWStructuredBuffer<VoxelHashEntry> nodeHashTable;
int nodeHashTableSize;

#endif

