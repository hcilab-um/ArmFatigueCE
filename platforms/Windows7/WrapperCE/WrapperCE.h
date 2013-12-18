// WrapperCE.h

#pragma once

#include "ArmFatigueCE.h"
#include "InterOp.h"

using namespace System;

namespace WrapperCE 
{
	public ref class EngineCE
	{
	private:
		FatigueEngine* engineCE;
		bool isStarted;
		Vector3D ConvertPV(InterOp::Point3D source);
		InterOp::Vector3D ConvertVP(Vector3D source);
		InterOp::FatigueData ConvertFatigueData(FatigueData fatigueData);
	public:
		EngineCE();
		~EngineCE();
		bool CheckStarted();
		void Start(WrapperCE::InterOp::UserGender gender);
		void Stop();
		WrapperCE::InterOp::ArmFatigueUpdate ProcessNewSkeletonData(InterOp::SkeletonData armsData, double deltaTimeInSeconds);
	};
}