using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class RubiksCube : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject[,,] RubikCube = new GameObject[3, 3, 3];

    public Vector3 cubeCenter;

    public float playerRotateDuration = 1.0f;
    public float autoRotateDuration = 1.0f;

    private const int ClockWise = 1;
    private const int CounterClockWise = -1;

    private const int FrontFace = 0;
    private const int BackFace = 2;

    private const int CubeSize = 3;
    private const int RotationsToShuffle = 10;

    private int _rotates;

    private bool _canRotate = true;

    private static readonly Vector2Int East = new Vector2Int(2, 0);
    private static readonly Vector2Int South = new Vector2Int(0, -2);
    private static readonly Vector2Int West = new Vector2Int(-2, 0);
    private static readonly Vector2Int North = new Vector2Int(0, 2);

    private static readonly Vector2Int Center = new Vector2Int(0, 0);

    private static readonly Vector2Int SouthEast = new Vector2Int(+1, -1);
    private static readonly Vector2Int SouthWest = new Vector2Int(-1, -1);
    private static readonly Vector2Int NorthWest = new Vector2Int(-1, +1);
    private static readonly Vector2Int NorthEast = new Vector2Int(+1, +1);

    private readonly Vector2Int[,] _kernelClockWiseRotation = new Vector2Int[,]
    {
        {
            East, SouthEast, South
        },
        {
            NorthEast, Center, SouthWest
        },
        {
            North, NorthWest, West
        }
    };

    private readonly Vector2Int[,] _kernelCounterClockWiseRotation = new Vector2Int[,]
    {
        {
            South, SouthWest, West
        },
        {
            SouthEast, Center, NorthWest
        },
        {
            East, NorthEast, North
        }
    };

    void Start()
    {
        for (int z = 0; z < CubeSize; z++)
        {
            for (int y = 0; y < CubeSize; y++)
            {
                for (int x = 0; x < CubeSize; x++)
                {
                    var position = new Vector3(x, y, z);
                    var cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                    cube.GetComponent<Cube>().originalPosition = new Vector3(x, y, z);
                    cube.name = $"X = {x}, Y = {y}, Z = {z}";

                    RubikCube[x, y, z] = cube;
                }
            }
        }

        var sizeX = RubikCube[2, 0, 0].transform.position.x - RubikCube[0, 0, 0].transform.position.x;
        var sizeY = RubikCube[0, 2, 0].transform.position.y - RubikCube[0, 0, 0].transform.position.y;
        var sizeZ = RubikCube[0, 0, 2].transform.position.z - RubikCube[0, 0, 0].transform.position.z;
        var center = new Vector3(sizeX / 2, sizeY / 2, sizeZ / 2);
        cubeCenter = center;

        StartCoroutine(Shuffle(autoRotateDuration));
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateRubikCube(Vector3.back, playerRotateDuration);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                RotateRubikCube(Vector3.left, playerRotateDuration);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotateRubikCube(Vector3.down, playerRotateDuration);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateRubikCube(Vector3.forward, playerRotateDuration);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            RotateRubikCube(Vector3.right, playerRotateDuration);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateRubikCube(Vector3.up, playerRotateDuration);
        }
        
        if (_canRotate)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.left, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (Input.GetKeyDown(KeyCode.S))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.left, CounterClockWise, BackFace, playerRotateDuration);
                }

                else if (Input.GetKeyDown(KeyCode.C))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.down, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (Input.GetKeyDown(KeyCode.D))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.down, CounterClockWise, BackFace, playerRotateDuration);
                }

                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.back, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (Input.GetKeyDown(KeyCode.A))
                {
                    RotateFace(_kernelCounterClockWiseRotation, Vector3.back, CounterClockWise, BackFace, playerRotateDuration);
                }
            }

            else if (Input.GetKeyDown(KeyCode.X))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.right, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (Input.GetKeyDown(KeyCode.S))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.right, ClockWise, BackFace, playerRotateDuration);
            }

            else if (Input.GetKeyDown(KeyCode.C))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.up, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (Input.GetKeyDown(KeyCode.D))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.up, ClockWise, BackFace, playerRotateDuration);
            }

            else if (Input.GetKeyDown(KeyCode.Z))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.forward, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (Input.GetKeyDown(KeyCode.A))
            {
                RotateFace(_kernelClockWiseRotation, Vector3.forward, ClockWise, BackFace, playerRotateDuration);
            }
        }
    }

    private void RotateFace(Vector2Int[,] kernel, Vector3 axisToRotate, int directionOfRotation, int depth, float rotateDuration)
    {
        _canRotate = false;

        var newCube = new GameObject[3, 3, 3];
        Array.Copy(RubikCube, newCube, RubikCube.Length);

        for (int firstElement = 0; firstElement < CubeSize; firstElement++)
        {
            for (int secondElement = 0; secondElement < CubeSize; secondElement++)
            {
                var origin = new Vector2Int(firstElement, secondElement);
                var move = kernel[firstElement, secondElement];
                var newPos = origin + move * directionOfRotation;

                var piece = new GameObject();

                if (axisToRotate == Vector3.right || axisToRotate == Vector3.left)
                {
                    piece = RubikCube[depth, firstElement, secondElement];
                    newCube[depth, newPos.x, newPos.y] = piece;
                    goto Chosen;
                }
                if (axisToRotate == Vector3.up || axisToRotate == Vector3.down)
                {
                    piece = RubikCube[secondElement, depth, firstElement];
                    newCube[newPos.y, depth, newPos.x] = piece;
                    goto Chosen;
                }
                if (axisToRotate == Vector3.forward || axisToRotate == Vector3.back)
                {
                    piece = RubikCube[firstElement, secondElement, depth];
                    newCube[newPos.x, newPos.y, depth] = piece;
                    goto Chosen;
                }

                Chosen:
                Rotate(piece.transform, axisToRotate, rotateDuration);
            }
        }

        RubikCube = newCube;
    }

    private void Rotate(Transform pieceTransform, Vector3 axis, float rotateDuration)
    {
        var rotateParent = new GameObject();
        rotateParent.transform.position = cubeCenter;
        pieceTransform.SetParent(rotateParent.transform);

        rotateParent.transform
            .DORotate(rotateParent.transform.eulerAngles + axis * 90, rotateDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .OnComplete(Reparent);

        void Reparent()
        {
            _canRotate = true;
            pieceTransform.SetParent(transform);
            Destroy(rotateParent);
        }

        _rotates++;

        if (_rotates == 9)
        {
            _rotates = 0;
            CheckVictory();
        }
    }

    private void RotateRubikCube(Vector3 axis, float rotateDuration)
    {
        // transform
        //     .DORotate(transform.eulerAngles + axis * 90, rotateDuration, RotateMode.LocalAxisAdd)
        //     .SetEase(Ease.Linear);
        transform.RotateAround(transform.position/2, axis, 90.0f);
    }

    private IEnumerator Shuffle(float duration)
    {
        var vectors = new Vector3[3];
        vectors[0] = Vector3.up;
        vectors[1] = Vector3.right;
        vectors[2] = Vector3.forward;

        var faces = new int[2];
        faces[0] = 0;
        faces[1] = 2;

        for (int i = 0; i < RotationsToShuffle; i++)
        {
            var randomVector = Random.Range(0, 3);
            var randomFace = Random.Range(0, 2);
            RotateFace(_kernelClockWiseRotation, vectors[randomVector], ClockWise, faces[randomFace], duration);
            yield return new WaitForSeconds(duration);
        }
    }

    private void CheckVictory()
    {
        for (int z = 0; z < CubeSize; z++)
        {
            for (int y = 0; y < CubeSize; y++)
            {
                for (int x = 0; x < CubeSize; x++)
                {
                    var actualPos = new Vector3(x, y, z);
                    var originalPos = RubikCube[x, y, z].GetComponent<Cube>().originalPosition;

                    if (actualPos != originalPos)
                    {
                        return;
                    }
                }
            }
        }

        Debug.Log("WIN!");
    }

    // private void RotateXFace(Vector2Int[,] kernel, Vector3 axisToRotate, int directionOfRotation, int depth)
    // {
    //     var newCube = CopyCube();
    //     
    //     for (int y = 0; y < 3; y++)
    //     {
    //         for (int x = 0; x < 3; x++)
    //         {
    //             var newPos = NewPosition(kernel, x, y, directionOfRotation);
    //
    //             var piece = RubikCube[depth, x, y];
    //             newCube[depth, newPos.x , newPos.y] = piece;
    //             
    //             Rotate(piece.transform, axisToRotate);
    //         }
    //     }
    //     
    //     RubikCube = newCube;
    // }
    //
    // private void RotateYFace(Vector2Int[,] kernel, Vector3 axisToRotate, int directionOfRotation, int depth)
    // {
    //     var newCube = CopyCube();
    //     
    //     for (int firstElement = 0; firstElement < 3; firstElement++)
    //     {
    //         for (int secondElement = 0; secondElement < 3; secondElement++)
    //         {
    //             var newPos = NewPosition(kernel, secondElement, firstElement, directionOfRotation);
    //
    //             var piece = RubikCube[firstElement, depth, secondElement];
    //             newCube[newPos.y, depth, newPos.x] = piece;
    //             
    //             Rotate(piece.transform, axisToRotate);
    //         }
    //     }
    //     
    //     RubikCube = newCube;
    // }
    //
    // private void RotateZFace(Vector2Int[,] kernel, Vector3 axisToRotate, int directionOfRotation, int depth)
    // {
    //     var newCube = CopyCube();
    //     
    //     for (int firstElement = 0; firstElement < 3; firstElement++)
    //     {
    //         for (int secondElement = 0; secondElement < 3; secondElement++)
    //         {
    //             var newPos = NewPosition(kernel, secondElement, firstElement, directionOfRotation);
    //
    //             var piece = RubikCube[secondElement, firstElement, depth];
    //             newCube[newPos.x, newPos.y, depth] = piece;
    //             
    //             Rotate(piece.transform, axisToRotate);
    //         }
    //     }
    //     
    //     RubikCube = newCube;
    // }

    // private GameObject[,,] CopyCube()
    // {
    //     lastSpin = 0;
    //     
    //     var newCube = new GameObject[3, 3, 3];
    //     Array.Copy(RubikCube, newCube, RubikCube.Length);
    //     return newCube;
    // }

    // private Vector2Int NewPosition( Vector2Int[,] kernel, int x, int y, int directionOfRotation)
    // {
    //     var origin = new Vector2Int(x, y);
    //     var move = kernel[x, y];
    //     var newPos = origin + move * directionOfRotation;
    //     return newPos;
    // }
}