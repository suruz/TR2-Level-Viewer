using UnityEngine;
using System.Collections;
/*
 * listing Lara's animation clips
 * 
TR2 Animations:

Movable 0: Basic Lara

0: Running
        0
        {footstep / walk in shallow water}
1: Walking carefully
        1
        {footstep / treading water}
2: Sidestep
        11
        {footstep / walk in shallow water}
3: Sidestep
        11
        {footstep / walk in shallow water}
4: Run a bit
        0
5: Run a bit
        0
6: Start running
        0
        {footstep / walk in shallow water}
7: Run to walk
        1
        {footstep / walk in shallow water}
8: Stop running
        11
        {footstep / walk in shallow water}
9: Run half a cycle
        1
        {footstep / walk in shallow water}
10: Stop running End a jump
        11
        {footstep / walk in shallow water}
11: Stationary
        103
12: Sidestep left
        12
        {footstep / treading water}
13: Sidestep right
        13
        {footstep / treading water}
14:     Landing on feet
        15
        {landing on feet / into water}
15: Getting up on feet
        11
        {Footstep / {}}
16: Jump
        17
        [Opcode 2]
        Jump {footstep / walk in shallow water}
17: Into the air
        77
18:     Jump
        19
        [Opcode 2]
        Jump {footstep / walk in shallow water}
19: Into the air
        77
20: Moving slightly leftward
        21
21: Leftward to forward
        1
        {footstep / treading water}
22: Landing
        23
23: Landing
        23
24: Landing, then getting up
        11
        {hitting wall, falling / walk in water, into water}
25: Dying
        25
        {dying with broken bones / into water}
26: Jump upwards
        27
        [Opcode 2]
        {{} / walk in shallow water}
27: Jump upwards
        28
        Jump
28: Jumping upwards
        28
29:     Grabbing
        [Opcode 1: Grab point]
        96
30: Landing
        23
31:     Landing
        11
        {landing on feet / walk in shallow water}
32: Pulling herself up
        33
33: Landing
        23
        Landing on feet, hitting wall
34: In the air
        35
35: In the air
        35
36: Jumping up
        23
37: Jumping up
        23
38: Getting to standstill
        11
        {footstep / walk in shallow water}
39: Getting to standstill
        11
        {footstep / walk in shallow water}
40: Walking backward
        40
        {footstep / treading water}
41: Starting to walk backward
        40
42: Pulling herself up a ledge
        11
        [Opcode 1: Step point]
        Pulling up {{} / surfacing}
43: Starting to run
        6
        [Opcode 1: Step point]
        [Opcode 3]
44: Sidestepping rightward
        44
        {footstep / trading water}
45: Falling
        23
46: Falling
        0
47: Flipover
        48
        {} / Into water
48: Flip-to-standing
        11
        [Opcode 6: 0]
        {footstep / walk in shallow water}
49: Falling
        23
50: Stepping up
        [Opcode 1: Step point]
        51
        Pulling up {{} / wade in shallow water}
51: To standing position
        11
        Footstep
52: Starting to run
        6
53: Hitting wall
        11
        Hitting wall
54: Hitting wall
        11
        Hitting wall
55: Stepping up
        0
        Footstep {{} / walk in shallow water}
56: Stepping up
        0
        Footstep {{} / walk in shallow water}
57: Stepping up
        1
        Footstep {{} / treading water}
58: Stepping down
        1
        Footstep {{} / treading water}
59: Stepping down
        1
        Footstep {{} / walk in shallow water}
60: Stepping down
        1
        Footstep {{} / walk in shallow water}
61: Stepping down backward
        40
        Footstep {{} / walk in shallow water}
62: Stepping down backward
        40
        Footstep {{} / walk in shallow water}
63: Pulling lever
        11
        [Opcode 3: attached to other]
        Pulling metal lever {{} / treading water}
64: Pushing lever back
        11
        [Opcode 3: attached to other]
        Metal-lever reset (pushing it back) {{} / treading water}
65: Sidestep left
        65
        Footstep {{} / treading water}
66: To standing position
        11
67: Sidestep right
        67
        Footstep {{} / treading water}
68: To standing position
        11
69: Step forward
        69
        Footstep {{} / treading water}
70: Sliding
        70
        Sliding
71: Recovering from slide
        72
        Footstep {{} / walk in shallow water}
72: To standing position
        11
        Footstep
73: Starting a standing jump
        91
        Jump {{} / treading water}
74: Jump
        [Opcode 2]
        {{} / surfacing}
75: Backflip
        75
76: Jump
        [Opcode 2]
        {{} / surfacing}
77: Landing
        77
78: Jump Left
        [Opcode 2]
        79
        {{} / surfacing}
79: Leftward Roll
        79
80: Jump Right
        [Opcode 2]
        81
        {{} / surfacing}
81: Rightward Roll
        81
82: Landing on Feet after a Fall
        11
        Landing on feet {{} / walk shallow, into water}
83: Falling
        23
84: Falling
        23
85: Falling
        23
86: Swimming
        [Opcode 6: 3, 3]
        Swimming
87: Swimming stationary
        87
88: Pushing something
        89
89: Jump backwards
        90
        Footstep {{} / walk in shallow water}
90: To standing position
        11
        Footstep {{} / treading water}
91: Jumping upwards
        [Opcode 2]
        28
        {{} / surfacing}
92: Running
        0
        Footstep {{} / walk in shallow water}
93: Fall
        93
94: Jump with getting ready to grab
        95
95: Jump with being ready to grab
        95
96: Grabbing a ledge
        96
        Ledge grab, pull up
97: Pulling herself up
        102
        [Opcode 1: Step point]
        Pulling up
98: Falling
        23
99: Landing
        82
        Landing on feet
100: Jump with being ready to grab
        95
101: Jump with being ready to grab
        95
102: Stationary
        11
103: Stationary and breathing
        103
104: Starting to slide while looking rightward
        105
        Landing on feet
105: Sliding while looking rightward
        105
        Sliding
106: To standing position
        11
        Footstep {{} / walk in shallow water}
107: Swimming
        108
108: Swimming
        108
        [Opcode 6: 3, 3, 3]
109: Swimming
        86
        Swimming
110: Treading water
        110
111: Pulling herself out of the water
        11
        [Opcode 6: 12]
        Surfacing
112: Diving
        86
        [Opcode 6: 3, 3, 3]
        Into water
113: Reverse direction underwater
        86
        Swimming, gurgle
114: Surfacing
        110
        Catching breath, surfacing
115: Starting to swim
        86
        Gurgle
116: Treading water
        116
        Treading water
117: Treading water
        110
118: Treading water
        116
119: Dive
        86
        [Opcode 6: 3, 3, 3]
        Swimming
120: Ready to push/pull block
        120
121: To standing position from push/pull
        11
122: Pulling block
        11
        [Opcode 1: Final-position offset]
        [Opcode 3: attached to other]
        Pulling block
123: Pushing block
        11
        [Opcode 1: Final-position offset]
        [Opcode 3: attached to other]
        Pushing block
124: Dying underwater
        124
        [Opcode 6: 3, 3, 3, 3]
125: Getting hurt
        125
        Getting hurt
126: Getting hurt
        126
        Getting hurt
126: Getting hurt
        127
        Getting hurt
128: Getting hurt
        0
        Getting hurt
129: Underwater lever pull
        108
        [Opcode 6: 3, 3, 3]
        Underwater lever pull, then swimming
130: Picking up underwater
        108
        [Opcode 6: 3, 3]
131: Pulling out and using key
        11
        [Opcode 3: attached to other]
        Inserting key/card {{} / treading water}
132: Dying underwater
        132
        [Opcode 6: 3, 3, 3]
133: Dying
        133
        Killed by flames/lava {{} / into water}
134: Inserting something
        11
        [Opcode 3: attached to other]
        Inserting chip
135: Picking something up
        11
        [Opcode 3: attached to other]
        Picked up something
136: Leftward on grabbed ledge
        136
        Something clicking
137: Rightward on grabbed ledge
        137
        Something clicking
138: Dying
        138
        Dying {{} / into water}
139: Dying
        139
        Getting hurt, hitting wall {{} / into water}
140: Starting to tread water
        141
141: Treading water backward
        141
        Treading water
142: Treading water
        110
143: Treading water leftward
        143
        Treading water
144: Treading water rightward
        144
        Treading water
145: Dying
        145
145: Dying, hitting ground {{} / into water}
146: Starting roll
        147
147: Finishing roll
        [Opcode 6: 0]
        {roll over / into water}
148: Standing up after roll
        11
149: Dying
        149
        Killed by spikes
150: Ledge grab
        96
        Ledge grab
151: Roll over
        11
        Hit the ground and roll {{} / into water}
152: Swimming
        87
        Into water
153: Dive
        153
154: Swimming
        87
        Into water
155: Dying
        155
        Dying with broken bones {{} / into water}
156: Starting dive
        158
157: Starting dive
        158
158: Swan dive
        158
159: Handstand pull-up
        102
        [Opcode 1: Step point]
        Pulling up [attachment point]
160: Starting to climb upward
        164
        [Opcode 1: Grab point]
        Pulling up, clicks, {{} / wade in shallow water}
161: Climbing upward
        161
        [Opcode 1: Grab point]
        Pulling up {{} / clicks}
162: Stop climbing upward
        164
        [Opcode 1: Grab point]
        Click
163: Stop climbing upward
        164
        [Opcode 1: Grab point]
        Click
164: Hold on while climbing
        164
165: Starting to climb upward
        161
        Click
166: Stop climbing downward
        164
        Click
167: Stop climbing downward
        164
        Click
168: Climb downward
        168
        [Opcode 1: Grab point]
        Click
169: Climb downward
        168
        [Opcode 1: Grab point]
        Click
170: Climb rightward
        170
        Click
171: Climb leftward
        171
        Click
172: Hanging on to ledge
        173
        [Opcode 1: Grab point]
173: Climbing up while holding ledge
        164
174: Climbing onto ledge
        102
        [Opcode 1:]
        Pulling up [attachment point, opcode 3]
175: To standing position
        11
176: Wading
        177
177: Wading
        177
        Wade in shallow water
178: Wading
        177
        Into water
179: Starting to wade
        177
180: Starting to run
        0
181: Starting to run
        0
182: Starting to jump backwards from grab
        183
        [Opcode 2]
183: Jumping backwards
        75
        [Opcode 1: Grab point]
184: To standing position
        11
185: To standing position
        11
186:
        177
187: Climb upward and stop
        96
        [Opcode 1: Grab point]
        Click
188: Climb downward and stop
        96
        [Opcode 1: Grab point]
        Click
189: Throwing flare
        189
190: Running uphill
        177
        Into water
191: Pulling upward
        11
        [Opcode 3]
        Into water
192: Getting onto terra firma
        11
        Into water
193: Getting onto terra firma
        11
        Into water
194: Climb down and grab
        96
        [Opcode 1: Grab point]
        Click
195: Pressing two buttons
        11
        [Opcode 3: attached to other]
        Lever pull
196: Pressing two buttons
        11
        [Opcode 3: attached to other]
        Lever pull
197: Pressing button
        11
        [Opcode 3: attached to other]
        Pressing button
198: Swimming
        108
199: Swimming
        108
200: Swimming
        108
201: Climb right and drop
        96
        [Opcode 1: Grab point]
202: Climb left and drop
        96
        [Opcode 1: Grab point]
203: Swimming flipover
        205
        Swimming
204: Pick up a flare
        11
205: Swimming
        108
206: Grabbing a flare while swimming
        108
207: Half-flipover
        209
        [Opcode 6: 0]
208: Flipover to dive
        153
209: Flipover
        75
210: Flipover
        211
        [Opcode 6: 0]
211: Landing after flip
        75
212: Flipover
        213
        [Opcode 6: 0]
213: Landing after flip
        77
214: Kicking
        11
215: Grabbing zipline handle
        [Opcode 1: Grab point]
        Zipline handle grab
216: Holding onto zipline
        216
217: Releasing zipline handle
        [Opcode 1: Grab point]
        [Opcode 3]
        77

Movable 1: Pistol Animations

218: Aim pistols
        218
219: Holster pistols
        218
220: Pull out pistols
        218
221: Aim pistols
        221

Movable 2: Ponytail

[All animations hardcoded as game physics]

Movable 3: Shotgun Animations

222: Aim shotgun
        224
223: Put shotgun back, grab shotgun
        222
        Holstering guns
224: Put shotgun back, grab shotgun
        224
        Firing shotgun, then cocking it
225: Put shotgun to hips
        225
        [Opcode 4]
        Holstering guns
226: Wield shotgun
        222

Movable 4: AutoPistol Animations

227: Aim autopistols
        227

Movable 5: Uzi Animations

228: Aim Uzis [has angry head]
        228

Movable 6: M16 Animations

229: M16 on back, aiming another M16
        231
230: Put M16 back, grab M16
        229
        Holstering guns
231: Fire M16
        231
232: Put M16 back, grab M16
        232
        [Opcode 4]
        Holstering guns
233: Turn M16 away
        229
234: Aim M16
        236
235: Turn M16 away
        234
236: Aim and fire M16
        236

Movable 7: Grenade-launcher animations

237: Put grenade launcher away, aim GL
        238
        Holstering guns
238: Aim grenade launcher
        240
239: Get grenade for launcher
        240
        Grenade settling, entering
240: Fire grenade launcher
        239
        Grenade-launcher firing
241: Turning grenade launcher away
        238
242: Put grenade launcher away, then wield it
        242
        [Opcode 4]
        Holstering guns

Movable 8: Harpoon-gun animations

243: Harpoon gun on back; aiming another HG
        245
244: Put harpoon gun back, grab HG
        243
        Holstering guns
245: Fire harpoon gun
        245
        Harpoon ricochet
246: Put harpoon gun back; grab HG
        243
        [Opcode 4]
        Holstering guns
247: Turn harpoon gun away
        243
248: Reload harpoon gun
        243
        {harpoon reload / underwater harpoon reload}
249: Pointing harpoon gun forward
        251
250: Turning harpoon gun away from forward
        249
251: Fire harpoon gun
        251
        Fire harpoon
252: Put harpoon gun back; grab HG
        252
        [Opcode 4]
253: Put harpoon gun back; grab HG
        253
        [Opcode 4]

Movable 9: Flare animations

254: Holding flare
        254
255: Pulling flare out
        255
256: Pulling flare out
        257
257: Lighting flare
        254
258: Lighting flare
        254
		*/




public class AnimationTester : MonoBehaviour {

	Animation anim = null;
	/*string Description ="" +
		"0: Running" +
		"1: Walking carefully" +
		"2: Sidestep" +
		"3: Sidestep" +
		"4: Run a bit" +
		"5: Run a bit" +
		"6: Start running" +
		"7: Run to walk" +
		"8: Stop running" +
		"9: Run half a cycle" +
		"10: Stop running End a jump" +
		"11: Stationary" +
		"12: Sidestep left" +
		"13: Sidestep right" +
		"14:     Landing on feet" +
		"15: Getting up on feet" +
		"16: Jump" +
		"17: Into the air" +
		"18:     Jump" +
		"19: Into the air" +
		"20: Moving slightly leftward" +
		"21: Leftward to forward" +
		"22: Landing" +
		"23: Landing" +
		"24: Landing, then getting up" +
		"25: Dying" +
		"26: Jump upwards" +
		"27: Jump upward" +
		"28: Jumping upwards" +
		"29:     Grabbing" +
		"30: Landing" +
		"31:     Landing" +
		"32: Pulling herself up" +
		"33: Landing" +
		"34: In the air" +
		"35: In the air" +
		"36: Jumping up" +
		"37: Jumping up" +
		"38: Getting to standstill" +
		"39: Getting to standstill" +
		"40: Walking backward" +
		"41: Starting to walk backward" +
		"42: Pulling herself up a ledge" +
		"43: Starting to run" +
		"44: Sidestepping rightward" +
		"45: Falling" +
		"46: Falling" +
		"47: Flipover" +
		"48: Flip-to-standing" +
		"49: Falling" +
		"50: Stepping up" +
		"51: To standing position" +
		"52: Starting to run" +
		"53: Hitting wall" +
		"54: Hitting wall" +
		"55: Stepping up" +
		"56: Stepping up" +
		"57: Stepping up" +
		"58: Stepping down" +
		"59: Stepping down" +
		"60: Stepping down" +
		"61: Stepping down backward" +
		"62: Stepping down backward" +
		"63: Pulling lever" +
		"64: Pushing lever back" +
		"65: Sidestep left" +
		"66: To standing position" +
		"67: Sidestep right" +
		"68: To standing position" +
		"69: Step forward" +
		"70: Sliding" +
		"71: Recovering from slide" +
		"72: To standing position" +
		"73: Starting a standing jump" +
		"74: Jump" +
		"75: Backflip" +
		"76: Jump" +
		"77: Landing" +
		"78: Jump Left" +
		"79: Leftward Roll" +
		"80: Jump Right" +
		"81: Rightward Roll" +
		"82: Landing on Feet after a Fall" +
		"83: Falling" +
		"84: Falling" +
		"85: Falling" +
		"86: Swimming" +
		"87: Swimming stationary" +
		"88: Pushing something" +
		"89: Jump backwards" +
		"90: To standing position" +
		"91: Jumping upwards" +
		"92: Running" +
		"93: Fall" +
		"94: Jump with getting ready to grab" +
		"95: Jump with being ready to grab" +
		"96: Grabbing a ledge" +
		"97: Pulling herself up" +
		"98: Falling" +
		"99: Landing" +
		"100: Jump with being ready to grab" +
		"101: Jump with being ready to grab" +
		"102: Stationary" +
		"103: Stationary and breathing" +
		"104: Starting to slide while looking rightward" +
		"105: Sliding while looking rightward" +
		"106: To standing position" +
		"107: Swimming" +
		"108: Swimming" +
		"109: Swimming" +
		"110: Treading water" +
		"111: Pulling herself out of the water" +
		"112: Diving" +
		"113: Reverse direction underwater" +
		"114: Surfacing" +
		"115: Starting to swim" +
		"116: Treading water" +
		"117: Treading water" +
		"118: Treading water" +
		"119: Dive" +
		"120: Ready to pushorpull block" +
		"121: To standing position from pushorpull" +
		"122: Pulling block" +
		"123: Pushing block" +
		"124: Dying underwater" +
		"125: Getting hurt" +
		"126: Getting hurt" +
		"126: Getting hurt" +
		"127: Getting hurt" +
		"128: Getting hurt" +
		"129: Underwater lever pull" +
		"130: Picking up underwater" +
		"131: Pulling out and using key" +
		"132: Dying underwater" +
		"133: Dying" +
		"134: Inserting something" +
		"135: Picking something up" +
		"136: Leftward on grabbed ledge" +
		"137: Rightward on grabbed ledge" +
		"138: Dying" +
		 139: Dying
        139
        Getting hurt, hitting wall [[] or into water]
140: Starting to tread water
        141
141: Treading water backward
        141
        Treading water
142: Treading water
        110
143: Treading water leftward
        143
        Treading water
144: Treading water rightward
        144
        Treading water
145: Dying
        145
145: Dying, hitting ground [[] or into water]
146: Starting roll
        147
147: Finishing roll
        [Opcode 6: 0]
        [roll over or into water]
148: Standing up after roll
        11
149: Dying
        149
        Killed by spikes
150: Ledge grab
        96
        Ledge grab
151: Roll over
        11
        Hit the ground and roll [[] or into water]
152: Swimming
        87
        Into water
153: Dive
        153
154: Swimming
        87
        Into water
155: Dying
        155
        Dying with broken bones [[] or into water]
156: Starting dive
        158
157: Starting dive
        158
158: Swan dive
        158
159: Handstand pull-up
        102
        [Opcode 1: Step point]
        Pulling up [attachment point]
160: Starting to climb upward
        164
        [Opcode 1: Grab point]
        Pulling up, clicks, [[] or wade in shallow water]
161: Climbing upward
        161
        [Opcode 1: Grab point]
        Pulling up [[] or clicks]
162: Stop climbing upward
        164
        [Opcode 1: Grab point]
        Click
163: Stop climbing upward
        164
        [Opcode 1: Grab point]
        Click
164: Hold on while climbing
        164
165: Starting to climb upward
        161
        Click
166: Stop climbing downward
        164
        Click
167: Stop climbing downward
        164
        Click
168: Climb downward
        168
        [Opcode 1: Grab point]
        Click
169: Climb downward
        168
        [Opcode 1: Grab point]
        Click
170: Climb rightward
        170
        Click
171: Climb leftward
        171
        Click
172: Hanging on to ledge
        173
        [Opcode 1: Grab point]
173: Climbing up while holding ledge
        164
174: Climbing onto ledge
        102
        [Opcode 1:]
        Pulling up [attachment point, opcode 3]
175: To standing position
        11
176: Wading
        177
177: Wading
        177
        Wade in shallow water
178: Wading
        177
        Into water
179: Starting to wade
        177
180: Starting to run
        0
181: Starting to run
        0
182: Starting to jump backwards from grab
        183
        [Opcode 2]
183: Jumping backwards
        75
        [Opcode 1: Grab point]
184: To standing position
        11
185: To standing position
        11
186:
        177
187: Climb upward and stop
        96
        [Opcode 1: Grab point]
        Click
188: Climb downward and stop
        96
        [Opcode 1: Grab point]
        Click
189: Throwing flare
        189
190: Running uphill
        177
        Into water
191: Pulling upward
        11
        [Opcode 3]
        Into water
192: Getting onto terra firma
        11
        Into water
193: Getting onto terra firma
        11
        Into water
194: Climb down and grab
        96
        [Opcode 1: Grab point]
        Click
195: Pressing two buttons
        11
        [Opcode 3: attached to other]
        Lever pull
196: Pressing two buttons
        11
        [Opcode 3: attached to other]
        Lever pull
197: Pressing button
        11
        [Opcode 3: attached to other]
        Pressing button
198: Swimming
        108
199: Swimming
        108
200: Swimming
        108
201: Climb right and drop
        96
        [Opcode 1: Grab point]
202: Climb left and drop
        96
        [Opcode 1: Grab point]
203: Swimming flipover
        205
        Swimming
204: Pick up a flare
        11
205: Swimming
        108
206: Grabbing a flare while swimming
        108
207: Half-flipover
        209
        [Opcode 6: 0]
208: Flipover to dive
        153
209: Flipover
        75
210: Flipover
        211
        [Opcode 6: 0]
211: Landing after flip
        75
212: Flipover
        213
        [Opcode 6: 0]
213: Landing after flip
        77
214: Kicking
        11
215: Grabbing zipline handle
        [Opcode 1: Grab point]
        Zipline handle grab
216: Holding onto zipline
        216
217: Releasing zipline handle
        [Opcode 1: Grab point]
        [Opcode 3]
        77

Movable 1: Pistol Animations

218: Aim pistols
        218
219: Holster pistols
        218
220: Pull out pistols
        218
221: Aim pistols
        221

Movable 2: Ponytail

[All animations hardcoded as game physics]

Movable 3: Shotgun Animations

222: Aim shotgun
        224
223: Put shotgun back, grab shotgun
        222
        Holstering guns
224: Put shotgun back, grab shotgun
        224
        Firing shotgun, then cocking it
225: Put shotgun to hips
        225
        [Opcode 4]
        Holstering guns
226: Wield shotgun
        222

Movable 4: AutoPistol Animations

227: Aim autopistols
        227

Movable 5: Uzi Animations

228: Aim Uzis [has angry head]
        228

Movable 6: M16 Animations

229: M16 on back, aiming another M16
        231
230: Put M16 back, grab M16
        229
        Holstering guns
231: Fire M16
        231
232: Put M16 back, grab M16
        232
        [Opcode 4]
        Holstering guns
233: Turn M16 away
        229
234: Aim M16
        236
235: Turn M16 away
        234
236: Aim and fire M16
        236

Movable 7: Grenade-launcher animations

237: Put grenade launcher away, aim GL
        238
        Holstering guns
238: Aim grenade launcher
        240
239: Get grenade for launcher
        240
        Grenade settling, entering
240: Fire grenade launcher
        239
        Grenade-launcher firing
241: Turning grenade launcher away
        238
242: Put grenade launcher away, then wield it
        242
        [Opcode 4]
        Holstering guns

Movable 8: Harpoon-gun animations

243: Harpoon gun on back; aiming another HG
        245
244: Put harpoon gun back, grab HG
        243
        Holstering guns
245: Fire harpoon gun
        245
        Harpoon ricochet
246: Put harpoon gun back; grab HG
        243
        [Opcode 4]
        Holstering guns
247: Turn harpoon gun away
        243
248: Reload harpoon gun
        243
        [harpoon reload or underwater harpoon reload]
249: Pointing harpoon gun forward
        251
250: Turning harpoon gun away from forward
        249
251: Fire harpoon gun
        251
        Fire harpoon
252: Put harpoon gun back; grab HG
        252
        [Opcode 4]
253: Put harpoon gun back; grab HG
        253
        [Opcode 4]

Movable 9: Flare animations

254: Holding flare
        254
255: Pulling flare out
        255
256: Pulling flare out
        257
257: Lighting flare
        254
258: Lighting flare
        254";
*/		

	//Use this for initialization
	void Start () 
	{
		anim = GetComponent<Animation>();
		anim.wrapMode = WrapMode.Clamp;
	}
	
	//Update is called once per frame
	int id = 0;
	void Update () 
	{
		if(anim != null && anim.GetClipCount() > 0) 
		{
			if(Input.GetKeyUp(KeyCode.UpArrow))
			{
				id++;

			}
		
			if(Input.GetKeyUp(KeyCode.DownArrow))
			{
				id--;
			}

			if(id >= anim.GetClipCount() )
			{
				id = anim.GetClipCount() - 1;
			}

			if(id < 0 )
			{
				id = 0;
			}
			Debug.Log("Clip: "+ id);
			anim.Play("" + id);
		}
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(10,100, Screen.width, Screen.height), "This is test scene to play Lara's animation clips manually\n" +
			"Press Up/ Down arrow key to change animation\n" +
			"Playing animation clip:" + id);
	}
}