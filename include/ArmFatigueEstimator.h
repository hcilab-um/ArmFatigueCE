#pragma once

#include "StdAfx.h"
#include "Vector3D.h"
#include "General.h"

#define HAND_MASS           0.4000

#define MALE_FOREARM_MASS   1.2000
#define FEMALE_FOREARM_MASS 1.000

#define MALE_UPPERARM_MASS      2.1000
#define FEMALE_UPPERARM_MASS    1.7000

#define MALE_UPPERARM_LENGHT    33.000
#define MALE_FOREARM_LENGHT     26.9000
#define MALE_HAND_LENGHT        19.1000

#define FEMALE_UPPERARM_LENGHT  31.000
#define FEMALE_FOREARM_LENGHT   23.4000
#define FEMALE_HAND_LENGHT      18.3000

#define UPPER_ARM_CENTER_GRAVITY_RATIO      0.452000
#define MALE_UPPER_ARM_CENTER_OF_GRAVITY    MALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO
#define FEMALE_UPPER_ARM_CENTER_OF_GRAVITY  FEMALE_UPPERARM_LENGHT * UPPER_ARM_CENTER_GRAVITY_RATIO

#define FOREARM_CENTER_GRAVITY_RATIO        0.424000
#define MALE_FOREARM_CENTER_OF_GRAVITY      MALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO
#define FEMALE_FOREARM_CENTER_OF_GRAVITY    FEMALE_FOREARM_LENGHT * FOREARM_CENTER_GRAVITY_RATIO

#define HAND_CENTER_GRAVITY_RATIO       0.397000
#define MALE_HAND_CENTER_OF_GRAVITY     MALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO
#define FEMALE_HAND_CENTER_OF_GRAVITY   FEMALE_HAND_LENGHT * HAND_CENTER_GRAVITY_RATIO

#define UPPER_ARM_INERTIA_RATE  0.0141000     //141*10^(-4)kg
#define FORE_ARM_INERTIA_RATE   0.0055000    //55*10^(-4)kg

#define GRAVITY_ACCELERATION    -9.8000 // m/s

#define MALE_MAX_FORCE      101.6000
#define FEMALE_MAX_FORCE    87.2000
#define CE_INFINITE            DBL_MAX

class EXPORT_OR_IMPORT ArmFatigueEstimator
{

private:

	static const Vector3D GRAVITY_VECTOR;

	FatigueData currentFatigueData;
	FatigueData lastFatigueData;

	double armMass;
	double maxForce;
	double maxTorque;
	double upperArmWeightProportion;
	double forearmAndHandCenterOfGravity;
	double foreArmAndHandCenterOfGravityRatio;
	double humanTorqueSum;

	Vector3D CalculateCenterMass(Vector3D shoulder, Vector3D elbow, Vector3D wrist, Vector3D hand);
	Vector3D CalculateInertialTorque(Vector3D displacement, Vector3D armCM, double angularAcc);
	double CalculateEndurance(double armStrength);

public:

	ArmFatigueEstimator(void);
	~ArmFatigueEstimator(void);

	void SetGenderValue(UserGender gender);
	FatigueData EstimateEffort(SkeletonData skeleton, Arm targetArm, double deltaTimeInSeconds, double totalTimeInSeconds);

	void Reset();
};