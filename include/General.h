#pragma once

#include <cfloat>

#include "stdafx.h"
#include "Vector3D.h"

enum UserGender { Male, Female };

enum Arm { LeftArm, RightArm };

struct Point3D
{
public:
	Point3D() {}

	Point3D(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	double X;
	double Y;
	double Z;
};

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

	Vector3D RightShoulderCms;
	Vector3D RightElbowCms;
	Vector3D RightHandCms;
	Vector3D LeftShoulderCms;
	Vector3D LeftElbowCms;
	Vector3D LeftHandCms;
};