using System.Collections.Generic;
using System;

namespace Mastermind;

class Program
{

    // 全域變數
    static Random rng = new Random();
    //預設猜測範圍0-99
    static int L = 0;
    static int H = 99;

    static void Main(string[] args)
    {
        // 1. Number-Guessing Game

        //guessNumberbyPlayer(ref L, ref H);

        // 1-1. Explore and determine the more effective strategy between binary search and random selection

        /*
         * binary search:永遠只會找中間，只有在answer最趨近於中點的時候才會贏，當數字範圍只剩下2-3個等同於亂數猜
         * random:範圍隨著隨機數變動變得越來越小，每個數字都有機會被選到，相較比binary search較容易選到極端值的answer
         */

        int totalTimes = 10000;//測試次數(1e5 是雙精度浮點數（double），如果要顯示整數只能打10000)
        int binarySearch = CalculateWinningRate(totalTimes, "binarySearch");
        int nativeGuess = CalculateWinningRate(totalTimes, "nativeGuess");

        Console.WriteLine($"Binary Search Winning Rate:{binarySearch}%");
        Console.WriteLine($"Native Guess Winning Rate:{nativeGuess}%");

        // 1-2. Find the optimal strategy

        //同樣做10000次的條件下:
        //binarySearch勝率:66%
        //nativeGuess勝率:63%

        /*
         * 策略一: 找出範圍，部分做隨機部分做二分
         * 勝率65-67%
         */
        int smartBinarySearch = CalculateWinningRate(totalTimes, "smartBinarySearch");
        Console.WriteLine($"Smart Binary Search勝率:{smartBinarySearch}%");

        /*
         *  策略二: 記取前面幾個答案的值然後取平均，小於特定範圍時採用隨機選擇
         *  勝率 63%
         */
        int avgAnswer = CalculateWinningRate(totalTimes, "avgAnswer");
        Console.WriteLine($"Average Anwser勝率:{avgAnswer}%");


        /*
        *  策略三: 永遠猜最小的數字(0-99只會在答案最後才會出現猜輸的狀況)
        *  勝率 99%
        */
        int smallest = CalculateWinningRate(totalTimes, "smallest");
        Console.WriteLine($"small to large勝率:{smallest}%");

        //1-3. 玩家只能玩7次(直接在CountingWinningTimes進行設定)

        /*
         * 在玩家只能限制7次的情況底下
         * binarySearchg是固定選擇2^7種選擇
         * nativeGuess最大猜測的組合為100^7種選擇(猜測範圍為0~99)
         * 而如果沒有限制猜測的話nativeGuess猜測的平均表現將更趨近於真實的機率分佈，而這可能有助於提高找到正確答案(大數法則)
         */

    }


    static void guessNumberbyPlayer(ref int L, ref int H)
    {

        Console.WriteLine($"終極密碼:從{L}到{H}猜一個數字");

        int answer = rng.Next(L, H + 1);//Random.Next設定範圍為Low <= num < High，故 H 要額外 + 1
        int totalTimes = 0;
        int updateL = L;
        int updateH = H;

        while (updateH > updateL)
        {
            // 累加玩家猜數字的次數
            totalTimes++;

            // 如果玩家輸入的是數字再進行細部判斷
            if (int.TryParse(Console.ReadLine(), out int playerGuess))
            {
                if (playerGuess > updateH || playerGuess < updateL)
                {
                    Console.WriteLine($"超出範圍，請猜{updateL}到{updateH}!");
                    continue;
                }
                else if (playerGuess > answer)
                {
                    updateH = playerGuess - 1;
                }
                else if (playerGuess < answer)
                {
                    updateL = playerGuess + 1;
                }
                else
                {
                    Console.WriteLine($"恭喜猜對了!!，答案就是{answer}");
                    break;
                }

                // 當選項在下一輪開始前發現已經只剩一個整數可選便輸了比賽
                if (updateH == updateL)
                {
                    Console.WriteLine($"可惜你失敗了，答案是{answer}");
                    break;
                }

                string message = playerGuess > answer ? "低" : "高";
                Console.WriteLine($"請再猜{message}一點，範圍從{updateL}到{updateH}");
            }
            else
            {
                Console.WriteLine("請輸入整數");
            }
        }

        Console.WriteLine($"此次遊玩你總共進行{totalTimes}次猜測。");
    }

    static int CalculateWinningRate(int totalTimes, string strategy)
    {
        int WinningTimes = 0;

        // 讓迴圈做{totalTimes}次
        for (int i = 0; i < totalTimes; i++)
        {
            int answer = rng.Next(H);
            WinningTimes += CountingWinningTimes(strategy, answer, ref L, ref H);
        }

        /* 
         * 機率計算 WinningTimes / totalTimes * 100
         * C#在整數相除時output也會是整數，也就是說0.01就會變成0，這樣勝率會永遠都是0
        */
        return (int)((double)WinningTimes / (double)totalTimes * 100);
    }

    static int CountingWinningTimes(string strategy, int answer, ref int L, ref int H)
    {
        int winningTime = 0;  // 記錄贏的次數
        int updateL = L;
        int updateH = H;
        int guess;
        int range = (int)Math.Floor(Math.Sqrt(H));
        //1-3限定玩家只能玩7次
        int guessTimes = 0;
        int limitTimes = 1000000;

        while (updateH > updateL)
        {
            switch (strategy)
            {
                case "binarySearch":
                    guess = (updateH + updateL) / 2;
                    break;
                case "nativeGuess":
                    guess = rng.Next(updateL, updateH + 1);
                    break;
                case "smartBinarySearch":
                    guess = updateH - updateL > range ? (updateH + updateL) / 2 : rng.Next(updateL, updateH + 1);
                    break;
                case "smallest":
                    guess = updateL;
                    break;
                case "avgAnswer":
                    int memoryCount = 1000;
                    List<int> answerArr = new List<int>();

                    if (answerArr.Count < memoryCount)
                    {
                        answerArr.Add(answer);
                        guess = (updateH + updateL) / 2;
                    }
                    else if (updateH - updateL < range)
                    {
                        guess = rng.Next(updateL, updateH + 1);
                    }
                    else
                    {
                        guess = (int)((double)answerArr.Sum() / (double)memoryCount);
                    }
                    break;
                default:
                    guess = 0;
                    break;
            }

            guessTimes++;

            if (guessTimes > limitTimes)
            {
                //如果次數超過規定就要跳出
                break;
            }
            else if (guess < answer)
            {
                updateL = guess + 1;

                continue;
            }
            else if (guess > answer)
            {
                updateH = guess - 1;
                continue;
            }
            else
            {
                // 猜對了就贏的次數+1
                winningTime++;
                break;
            }
        }

        return winningTime;
    }

}