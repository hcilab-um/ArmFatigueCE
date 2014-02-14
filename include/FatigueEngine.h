#pragma once

#include "General.h"
#include "ArmFatigueEstimator.h"

class EXPORT_OR_IMPORT FatigueEngine
{
private:

	double totalTime;

	UserGender gender;

	ArmFatigueEstimator estimatorLeft;
	ArmFatigueEstimator estimatorRight;

public:
	FatigueEngine(void);
	~FatigueEngine(void);

	void SetGender(UserGender newGender);
	UserGender GetGender();
	void Reset();
	
	Vector3D EstimateWristPosition(Vector3D hand, Vector3D elbow);
	ArmFatigueUpdate ProcessNewSkeletonData(SkeletonData skeleton, double deltaTimeInSeconds);
};

