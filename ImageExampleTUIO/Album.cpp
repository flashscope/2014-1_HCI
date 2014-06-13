#include "Album.h"


Album::Album( std::string imagePath, float positionX, float positionY, float angle, int group )
	: MovingObject( imagePath, positionX, positionY, angle, group)
{
	m_IsVisible = true;
	m_MOType = MO_TYPE_ALBUM;
}


Album::~Album(void)
{
}

void Album::Draw()
{
	m_IsVisible = true;
	MovingObject::Draw();
}
