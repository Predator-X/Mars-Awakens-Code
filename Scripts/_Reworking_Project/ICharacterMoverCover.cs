using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterMoverCover 
{
    bool inCover { get; set; }

    void BeginMoveToCover(Vector3 targetPos);

    Vector3 inCoverMoveDirection { get; set; }
    Vector3 inCoverProhebitedDirection { get; set; }
}
