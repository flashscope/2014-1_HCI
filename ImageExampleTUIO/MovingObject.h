#pragma once
#include "ofMain.h"

enum MovingObjectType {
	MO_TYPE_NONE,
	MO_TYPE_ALBUM,
	MO_TYPE_PHOTO,
	MO_TYPE_MAX,
};

class MovingObject
{
public:
	MovingObject( std::string imagePath, float positionX, float positionY, float angle, int group);
	virtual ~MovingObject(void);


	virtual void	Draw();
	//void	AddChild( MovingObject* child ) { m_MovingObjectChildList.push_back(child); }

	bool	IsCollide( int x, int y );

	float	GetPositionX() { return m_PositionX; }
	float	GetPositionY() { return m_PositionY; }
	float	GetSizeWidth() { return m_SizeWidth; }
	float	GetSizeHeight() { return m_SizeHeight; }
	float	GetAngle() { return m_Angle; }
	int		GetGroup() { return m_Group; }
	MovingObjectType GetMOType() { return m_MOType; }
	bool	GetOpen() { return m_Open; }
	unsigned long long GetLastCheckTime() { return m_LastCheckTime; }



	void	SetPosition(float x, float y) { m_PositionX = x; m_PositionY = y; }
	void	SetSize(float width, float height) { m_SizeWidth = width; m_SizeHeight = height; }
	void	AddSize(float size) { m_SizeWidth += size; m_SizeHeight += size; }



	void	SetAngle(float angle ) { m_Angle = angle; }
	void	AddAngle(float addAngle ) { m_Angle += addAngle; }


	void	SetVisible(bool visible) { m_IsVisible = visible; }
	void	SetOpen(bool open) { m_Open = open; } // album open
	void	SetCheckTime() { m_LastCheckTime = ofGetSeconds(); }
	void	SetMovePositionTemp() { m_PositionXTemp = m_PositionX; m_PositionYTemp = m_PositionY; }

	void	MoveBackToTempPosition() { 
		m_PositionX = m_PositionXTemp; 
	m_PositionY = m_PositionYTemp; }

protected:

	ofImage m_MyImage1;

	float m_PositionX;
	float m_PositionY;

	float m_SizeWidth;
	float m_SizeHeight;

	float m_Angle;

	MovingObjectType m_MOType;
	int m_Group;
	bool m_IsVisible;
	bool m_Open;
	//std::vector<MovingObject*> m_MovingObjectChildList;

	unsigned long long m_LastCheckTime;

	float m_PositionXTemp;
	float m_PositionYTemp;
};

