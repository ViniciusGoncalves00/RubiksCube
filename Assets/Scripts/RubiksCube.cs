using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class RubiksCube : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject[,,] RubikCube = new GameObject[3, 3, 3];

    private Vector3 _cubeCenter;

    private float playerRotateDuration = 0.3f;
    private float autoRotateDuration = 0.15f;

    private const int ClockWise = 1;
    private const int CounterClockWise = -1;

    private const int FrontFace = 0;
    private const int BackFace = 2;

    private const int CubeSize = 3;
    private const int RotationsToShuffle = 10;

    private int _rotationsToVerifyVictoryCondition;

    private bool _canRotate = true;

    private Vector3 _initialRotation;

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
        CreateCube();

        _initialRotation = _cubeCenter;

        StartCoroutine(Shuffle(autoRotateDuration));
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateCube(Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                RotateCube(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotateCube(Vector3.down);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateCube(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            RotateCube(Vector3.right);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateCube(Vector3.up);
        }
        
        if (_canRotate)
        {
            if (InputSystem.PressedSwitch)
            {
                if (InputSystem.R01)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.left, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (InputSystem.R02)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.left, CounterClockWise, BackFace, playerRotateDuration);
                }

                else if (InputSystem.R03)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.down, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (InputSystem.R04)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.down, CounterClockWise, BackFace, playerRotateDuration);
                }

                else if (InputSystem.R05)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.back, CounterClockWise, FrontFace, playerRotateDuration);
                }

                else if (InputSystem.R06)
                {
                    RotateFaceArray(_kernelCounterClockWiseRotation, Vector3.back, CounterClockWise, BackFace, playerRotateDuration);
                }
            }

            else if (InputSystem.R01)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.right, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (InputSystem.R02)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.right, ClockWise, BackFace, playerRotateDuration);
            }

            else if (InputSystem.R03)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.up, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (InputSystem.R04)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.up, ClockWise, BackFace, playerRotateDuration);
            }

            else if (InputSystem.R05)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.forward, ClockWise, FrontFace, playerRotateDuration);
            }

            else if (InputSystem.R06)
            {
                RotateFaceArray(_kernelClockWiseRotation, Vector3.forward, ClockWise, BackFace, playerRotateDuration);
            }
        }

        if (InputSystem.DefaultRotation)
        {
            RotateCube(_initialRotation);
        }
    }

    private void CreateCube()
    {
        for (int z = 0; z < CubeSize; z++)
        {
            for (int y = 0; y < CubeSize; y++)
            {
                for (int x = 0; x < CubeSize; x++)
                {
                    var position = new Vector3(x, y, z);
                    var cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                    cube.GetComponent<Cube>().OriginalPosition = new Vector3(x, y, z);
                    cube.name = $"X = {x}, Y = {y}, Z = {z}";

                    RubikCube[x, y, z] = cube;
                }
            }
        }

        var sizeX = RubikCube[2, 0, 0].transform.position.x - RubikCube[0, 0, 0].transform.position.x;
        var sizeY = RubikCube[0, 2, 0].transform.position.y - RubikCube[0, 0, 0].transform.position.y;
        var sizeZ = RubikCube[0, 0, 2].transform.position.z - RubikCube[0, 0, 0].transform.position.z;
        var center = new Vector3(sizeX / 2, sizeY / 2, sizeZ / 2);
        _cubeCenter = center;
    }

    private void RotateFaceArray(Vector2Int[,] kernel, Vector3 axisToRotate, int directionOfRotation, int depth, float rotateDuration)
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
                }
                else if (axisToRotate == Vector3.up || axisToRotate == Vector3.down)
                {
                    piece = RubikCube[secondElement, depth, firstElement];
                    newCube[newPos.y, depth, newPos.x] = piece;
                }
                else if (axisToRotate == Vector3.forward || axisToRotate == Vector3.back)
                {
                    piece = RubikCube[firstElement, secondElement, depth];
                    newCube[newPos.x, newPos.y, depth] = piece;
                }
                
                RotateFace(piece.transform, axisToRotate, rotateDuration);
            }
        }

        RubikCube = newCube;
    }

    private void RotateFace(Transform pieceTransform, Vector3 axis, float rotateDuration)
    {
        var rotateParent = new GameObject();
        rotateParent.transform.position = _cubeCenter;
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

        _rotationsToVerifyVictoryCondition++;

        if (_rotationsToVerifyVictoryCondition == 9)
        {
            _rotationsToVerifyVictoryCondition = 0;
            CheckVictory();
        }
    }
    
    private void RotateCube(Vector3 axis)
    {
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
            RotateFaceArray(_kernelClockWiseRotation, vectors[randomVector], ClockWise, faces[randomFace], duration);
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
                    var originalPos = RubikCube[x, y, z].GetComponent<Cube>().OriginalPosition;

                    if (actualPos != originalPos)
                    {
                        return;
                    }
                }
            }
        }
    }
}