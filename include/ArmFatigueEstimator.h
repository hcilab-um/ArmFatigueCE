#pragma once

#include "StdAfx.h"
#include "Vector3D.h"
#include "General.h"

#define HAND_MASS           0.4
#define MALE_FOREARM_MASS   1.2
#define FEMALE_FOREARM_MASS 1.0

#define MALE_UPPERARM_MASS      2.1
#define FEMALE_UPPERARM_MASS    1.7

#define MALE_UPPERARM_LENGHT    33
#define MALE_FOREARM_LENGHT     26.9
#define MALE_HAND_LENGHT        19.1

#define FEMALE_UPPERARM_LENGHT  31
#define FEMALE_FOREARM_LENGHT   23.4
#define FEMALE_HAND_LENGHT      18.3

#define UPPER_ARM_CENTER_GRAVITY_RATIO      0.452
#define MALE_UPPER_ARM_CENTER_OF_GRAVITY    MALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO
#define FEMALE_UPPER_ARM_CENTER_OF_GRAVITY  FEMALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO

#define FOREARM_CENTER_GRAVITY_RATIO        0.424
#define MALE_FOREARM_CENTER_OF_GRAVITY      MALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO
#define FEMALE_FOREARM_CENTER_OF_GRAVITY    FEMALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO

#define HAND_CENTER_GRAVITY_RATIO       0.397
#define MALE_HAND_CENTER_OF_GRAVITY     MALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO
#define FEMALE_HAND_CENTER_OF_GRAVITY   FEMALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO

#define UPPER_ARM_INERTIA_RATE  0.0141     //141*10^(-4)kg
#define FORE_ARM_INERTIA_RATE   0.0055    //55*10^(-4)kg

#define GRAVITY_ACCELERATION    -9.8 // m/s

#define MALE_MAX_FORCE      101.6
#define FEMALE_MAX_FORCE    87.2
#define INFINITE            DBL_MAX

class EXPORT_OR_IMPORT ArmFatigueEstimator
{

private:

	static const Vector3D GRAVITY_VECTOR;

	FatigueData lastFatigueData;
	double armMass;
	double maxForce;
	double maxTorque;
	double upperArmWeightProportion;
	double forearmAndHandCenterOfGravity;
	double foreArmAndHandCenterOfGravityRatio;
	double humanTorqueSum;

	Vector3D CalculateCenterMass(Vector3D shoulder, Vector3D elbow, Vector3D hand);
	
	Vector3D CalculateInertialTorque(Vector3D displacement, Vector3D armCM, double angularAcc);

	double CalculateEndurance(double shoulderTorquePercent);

public:

	ArmFatigueEstimator(void);
	~ArmFatigueEstimator(void);

	void SetGenderValue(UserGender gender);
	FatigueData EstimateEffort(SkeletonData skeleton, double deltaTime, double totalTime);

	void Reset();
	
};