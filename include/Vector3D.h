#pragma once

#include "stdafx.h"

class EXPORT_OR_IMPORT Vector3D
{
public:
	double X;
	double Y;
	double Z;
	
	static double CalculateAngleInDegrees(Vector3D vector1, Vector3D vector2);
	static Vector3D CrossProduct(Vector3D vector1, Vector3D vector2);
	static double DotProduct(Vector3D vector1, Vector3D vector2);
	
	Vector3D operator + (const Vector3D& vector) const;
	Vector3D operator - (const Vector3D& vector) const;
	Vector3D operator * (const double& scalar) const;      
	Vector3D operator / (const double& scalar) const;

	Vector3D(void);
	Vector3D(double x, double y, double z);
	~Vector3D(void);

	double GetLength();
	void Normalize();
};

