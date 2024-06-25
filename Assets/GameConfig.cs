using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameConfig
{
    public float cameraShakePower;
    public float cameraShakeTime;
    public float cameraMoveForwardOnNoteSpawnSpeed;
    public float lightsIntencity;
    public float lightsAngle;
    public float lightsTurnOnSpeed;
    public float lightsDimsSpeed;
    public float lightsMinimumDim;

    public float noteYPositionMultiplier;

    public float cubeNoteJumpTime;
    public float cubeNoteJumpPower;

    public float cubeNoteScaleTime;
    public float cubeNoteScale;

    public float cubeNoteGradientTime1;
    public Vector4 cubeNoteGradientColor1;
    public float cubeNoteGradientTime2;
    public Vector4 cubeNoteGradientColor2;
    public float cubeNoteGradientTime3;
    public Vector4 cubeNoteGradientColor3;

    public void ResetToDefault()
    {

        cameraShakePower = 0.1f;
        cameraShakeTime = 0.5f;
        cameraMoveForwardOnNoteSpawnSpeed = 1.0f;


    lightsIntencity = 4.0f;
        lightsAngle = 16.0f;
        lightsTurnOnSpeed = 0.1f;
        lightsDimsSpeed = 4.0f;
        lightsMinimumDim = 0.2f;

        noteYPositionMultiplier = 1.0f;
        cubeNoteJumpTime = 0.5f;
        cubeNoteJumpPower = 2;
        cubeNoteScaleTime = 0.5f;
        cubeNoteScale = 1f;

        cubeNoteGradientTime1 = 0.2f;
        cubeNoteGradientColor1 = Color.red;
        cubeNoteGradientTime2 = 0.5f;
        cubeNoteGradientColor2 = Color.green;
        cubeNoteGradientTime3 = 1.0f;
        cubeNoteGradientColor3 = Color.blue;
    }
}
