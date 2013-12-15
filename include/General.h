#pragma once

#include <cfloat>

#include "stdafx.h"
#include "Vector3D.h"

enum UserGender { Male, Female };

enum Arm { LeftArm, RightArm };

struct FatigueData
{
public:
	Arm TargetArm;

	double Theta;
	Vector3D CenterOfMass;
	Vector3D Displacement;
	Vector3D Velocity;
	Vector3D Acceleration;
	Vector3D InertialTorque;

	double AngularAcceleration;
	double ShoulderTorque;
	double Endurance;

	double AvgShoulderTorque;
	double AvgEndurance;

	double ConsumedEndurance;
};

struct ArmFatigueUpdate
{
public:
	FatigueData LeftArm;
	FatigueData RightArm;
};

struct SkeletonData
{
public:
	SkeletonData() 
	{ }

	Vector3D rightShoulderCms;
	Vector3D rightElbowCms;
	Vector3D rightHandCms;
	Vector3D leftShoulderCms;
	Vector3D leftElbowCms;
	Vector3D leftHandCms;
};