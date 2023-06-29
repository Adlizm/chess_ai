# -*- coding: utf-8 -*-
import chess
import chess.engine
import chess.pgn
from datetime import datetime
from tqdm import tqdm


f = open("results.pgn")

draw = 0
white_win = 0
black_win = 0

while True:
    game = chess.pgn.read_game(f)
    if game is None:
        break  # end of file
    result = game.headers["Result"]

    if result == "1-0":
        white_win += 1
    elif result == "0-1":
        black_win += 1
    else:
        draw += 1


print("\n")
print("Draw: {}".format(draw))
print("White win: {}".format(white_win))
print("Black win: {}".format(black_win))

