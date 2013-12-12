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
		Point3D Convert(WrapperCE::InterOp::Point3D source);

	public:
		EngineCE();
		~EngineCE();

		WrapperCE::InterOp::ArmFatigueUpdate ProcessNewSkeletonData(WrapperCE::InterOp::SkeletonData armsData);
	};
}