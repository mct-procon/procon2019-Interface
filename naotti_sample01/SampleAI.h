#pragma once
#include "NIIGATA.h"
#include "Move.h"

class SampleAI {
public:
	//move1 … status.player[2 * teamId]の行動 (書き込む)
	//move2 … status.player[2 * teamId + 1]の行動 (書き込む)
	void think(NIIGATA status, int teamId, Move &move1, Move &move2);
};