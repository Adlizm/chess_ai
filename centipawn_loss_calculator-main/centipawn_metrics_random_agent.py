# -*- coding: utf-8 -*-
import chess
import chess.engine
import chess.pgn
from datetime import datetime
from tqdm import tqdm

import random

def evaluate_game(board, engine, limit):
   info = engine.analyse(board, limit)
   return info['score'].white().score(mate_score=1000)

start = datetime.now()

# put here your path to your engine on your computer
engine = chess.engine.SimpleEngine.popen_uci('C:\\Users\\PICHAU\\Documents\\stockfish_15.1_win_x64_popcnt\\stockfish-windows-2022-x86-64-modern') 


movetimesec = 999
depth = 20
limit = chess.engine.Limit(time=movetimesec, depth=depth)

random_games = 50
limit_moves_game = 120

white_centipawn_loss_list = []
black_centipawn_loss_list = []

for i in tqdm(range(50)):
    try:
        # Evaluate all moves
        board = chess.Board()
        
        evaluations = []
        
        evaluation = engine.analyse(board, limit=limit)['score'].white().score()
        evaluations.append(evaluation)
        
        moves_made = 0
        while not board.is_game_over() and moves_made < limit_moves_game:
            move = random.choice([move for move in board.legal_moves])
            board.push(move)
            positionEvaluation = evaluate_game(board, engine, limit)
            evaluations.append(positionEvaluation)
            moves_made += 1
        
        # Adjust evaluations
        evaluationsAdjusted = evaluations.copy()
        evaluationsAdjusted = [max(min(x, 1000), -1000) for x in evaluationsAdjusted]
        
        index = 0
        for singleEvaluation in evaluationsAdjusted:
            if index > 0:
                previous_state_evaluation = evaluationsAdjusted[index - 1]
                current_state_evaluation = evaluationsAdjusted[index]    
                if index % 2 != 0:
                    white_centipawn_loss_list.append(previous_state_evaluation - current_state_evaluation)
                else:
                    black_centipawn_loss_list.append(current_state_evaluation - previous_state_evaluation)
            index += 1
    except Exception as e:
        print(e)
        pass


white_centipawn_loss_list_adjusted = [0 if x < 0 else x for x in white_centipawn_loss_list]
black_centipawn_loss_list_adjusted = [0 if x < 0 else x for x in black_centipawn_loss_list]

white_average_centipawn_loss = round(sum(white_centipawn_loss_list_adjusted) / len(white_centipawn_loss_list_adjusted))
black_average_centipawn_loss = round(sum(black_centipawn_loss_list_adjusted) / len(black_centipawn_loss_list_adjusted))

print("\n\n")
print("White average centipawn loss: {}".format(white_average_centipawn_loss))
print("Black average centipawn loss: {}".format(black_average_centipawn_loss))

#####################
print("Done! Job complete!")
#####################
#####################
finish = datetime.now()
print('It took long but it\'s done! The entire job took: {}'.format(finish - start))
#####################
#####################

