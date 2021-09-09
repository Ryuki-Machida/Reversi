using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Field : MonoBehaviour
{
    /// <summary>セルのプレハブ</summary>
    [SerializeField] Piece m_piecePrefab;
    [SerializeField] GridLayoutGroup m_container = null;
    /// <summary>横の長さ</summary>
    [SerializeField] int m_row = default;
    /// <summary>縦の長さ</summary>
    [SerializeField] int m_col = default;
    /// <summary>各色の数</summary>
    [SerializeField] Text m_whiteText = null;
    [SerializeField] Text m_blackText = null;
    [SerializeField] Text m_turnText = null;
    /// <summary>ステージの配列</summary>
    private Piece[,] m_piece;
    [SerializeField] GameObject m_panel = null;
    [SerializeField] Text m_panelText = null;
    [SerializeField] string m_GameRecord = "";
    bool m_turnFlag = true;
    bool m_change = true;

    private bool firstPush = false;

    void Start()
    {
        if (m_col < m_row)
        {
            m_container.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            m_container.constraintCount = m_row;
        }
        else
        {
            m_container.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            m_container.constraintCount = m_col;
        }
        m_piece = new Piece[m_row, m_col];

        //ステージを生成
        for (int col = 0; col < m_piece.GetLength(1); col++)
        {
            for (int row = 0; row < m_piece.GetLength(0); row++)
            {
                var piece = Instantiate(m_piecePrefab);
                var parent = m_container.transform;
                piece.transform.SetParent(parent);
                m_piece[row, col] = piece;
                piece.GetCoordinate(row, col);
                if (row == 3 && col == 3 || row == 4 && col == 4)
                {
                    var _piece = m_piece[row, col];
                    _piece.State = State.StateOpen;
                    _piece.PieceState = PieceState.White;
                }
                if (row == 3 && col == 4 || row == 4 && col == 3)
                {
                    var _piece = m_piece[row, col];
                    _piece.State = State.StateOpen;
                    _piece.PieceState = PieceState.Black;
                }
            }
        }
        Aggregate();
        AllSearch();
        m_turnText.text = "白の番";
    }

    /// <summary>
    /// 順番を交換
    /// </summary>
    public void ChangeTurn(int r, int c)
    {
        if (m_turnFlag == true)
        {
            m_piece[r, c].State = State.Open;
            m_piece[r, c].PieceState = PieceState.White;
            m_turnFlag = false;
            m_turnText.text = "黒の番";
        }
        else
        {
            m_piece[r, c].State = State.Open;
            m_piece[r, c].PieceState = PieceState.Black;
            m_turnFlag = true;
            m_turnText.text = "白の番";
        }
        AllSearch();
    }

    /// <summary>
    /// 置ける場所をすべて探すパターン
    /// </summary>
    public void AllSearch()
    {
        int AllChangeCount = 0;
        for (int c = 0; c < m_col; c++)
        {
            for (int r = 0; r < m_row; r++)
            {
                int ChangeCount = 0;
                ChangeCount += Search(r, c, -1, 0);//左
                ChangeCount += Search(r, c, -1, -1);//左上
                ChangeCount += Search(r, c, 0, -1);//上
                ChangeCount += Search(r, c, 1, -1);//右上
                ChangeCount += Search(r, c, 1, 0);//右
                ChangeCount += Search(r, c, 1, 1);//右下
                ChangeCount += Search(r, c, 0, 1);//下
                ChangeCount += Search(r, c, -1, 1);//左下
                Debug.Log(ChangeCount);
                if (ChangeCount != 0 && m_piece[r, c].State == State.Close)
                {
                    //そのセルの色を変える
                    m_piece[r, c].GetComponent<Image>().color = Color.gray;
                    AllChangeCount++;
                }
                else
                {
                    m_piece[r, c].GetComponent<Image>().color = Color.white;
                }
            }
        }
        if (AllChangeCount == 0)
        {
            if (m_turnFlag == true)
            {
                m_turnFlag = false;
            }
            else
            {
                m_turnFlag = true;
            }
        }
    }

    /// <summary>
    /// 置ける場所を探す
    /// </summary>
    public int Search(int r, int c, int moveR, int moveC)
    {
        int row = r + moveR;
        int col = c + moveC;
        int checkCount = 0;
        while (row < m_row && col < m_col || row >= 0 && col >= 0)//配列の外側に出たら終わる
        {
            if (row == -1 || col == -1 || row == m_row || col == m_col)
            {
                break;
            }
            if (m_piece[row, col].PieceState == PieceState.None)
            {
                break;
            }
            if (m_turnFlag == true)
            {

                if (m_piece[row, col].PieceState == PieceState.Black)
                {
                    row += moveR;
                    col += moveC;
                    checkCount++;
                }
                else if (m_piece[row, col].PieceState == PieceState.White)
                {
                    if (checkCount != 0)
                    {
                        return 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (m_piece[row, col].PieceState == PieceState.White)
                {
                    row += moveR;
                    col += moveC;
                    checkCount++;
                }
                else if (m_piece[row, col].PieceState == PieceState.Black)
                {
                    if (checkCount != 0)
                    {
                        return 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// 調べる全てのパターン
    /// </summary>
    public void PieceAllChange(int r, int c)
    {
        int ChangeCount = 0;
        ChangeCount += PieceChange(r, c, -1, 0);//左
        ChangeCount += PieceChange(r, c, -1, -1);//左上
        ChangeCount += PieceChange(r, c, 0, -1);//上
        ChangeCount += PieceChange(r, c, 1, -1);//右上
        ChangeCount += PieceChange(r, c, 1, 0);//右
        ChangeCount += PieceChange(r, c, 1, 1);//右下
        ChangeCount += PieceChange(r, c, 0, 1);//下
        ChangeCount += PieceChange(r, c, -1, 1);//左下

        if (ChangeCount == 0)
        {
            Debug.Log("ここには置けない");
        }
        else
        {
            ChangeTurn(r, c);
        }
        Aggregate();
    }

    /// <summary>
    /// 駒を置いた所の各方向を調べて返す。
    /// </summary>
    public int PieceChange(int r, int c, int moveR, int moveC)
    {
        int row = r + moveR;
        int col = c + moveC;
        int checkCount = 0;
        while (row < m_row && col < m_col || row >= 0 && col >= 0)//配列の外側に出たら終わる
        {
            if (row == -1 || col == -1 || row == m_row || col == m_col)
            {
                break;
            }
            if (m_piece[row, col].PieceState == PieceState.None)
            {
                break;
            }
            if (m_turnFlag == true)
            {
                if (m_piece[row, col].PieceState == PieceState.Black)
                {
                    row += moveR;
                    col += moveC;
                    checkCount++;
                }
                else if (m_piece[row, col].PieceState == PieceState.White)
                {
                    if (checkCount != 0)
                    {
                        int ChangeRow = r + moveR;
                        int ChangeCol = c + moveC;
                        while (!(ChangeRow == row && ChangeCol == col))
                        {
                            m_piece[ChangeRow, ChangeCol].PieceState = PieceState.White;
                            ChangeRow += moveR;
                            ChangeCol += moveC;
                        }
                        return 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (m_piece[row, col].PieceState == PieceState.White)
                {
                    row += moveR;
                    col += moveC;
                    checkCount++;
                }
                else if (m_piece[row, col].PieceState == PieceState.Black)
                {
                    if (checkCount != 0)
                    {
                        int ChangeRow = r + moveR;
                        int ChangeCol = c + moveC;
                        while (!(ChangeRow == row && ChangeCol == col))
                        {
                            m_piece[ChangeRow, ChangeCol].PieceState = PieceState.Black;
                            ChangeRow += moveR;
                            ChangeCol += moveC;
                        }
                        return 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// 駒の数を更新し、必要があればゲームを終了させる。
    /// </summary>
    void Aggregate()
    {
        int white = 0;
        int black = 0;
        int none = 0;
        foreach (var i in m_piece)
        {
            if (i.PieceState == PieceState.Black)
            {
                black++;
            }
            else if (i.PieceState == PieceState.White)
            {
                white++;
            }
            else
            {
                none++;
            }
        }
        m_whiteText.text = "白：" + white;
        m_blackText.text = "黒：" + black;
        if (none == 0 || white == 0 || black == 0)
        {
            GameSet(white, black);
        }
    }

    /// <summary>
    /// 勝利判定のテキスト
    /// </summary>
    void GameSet(int white, int black)
    {
        m_panel.SetActive(true);
        if (white == black)
        {
            m_panelText.text = "引き分け！";
        }
        else if (white > black)
        {
            m_panelText.text = "白の勝ち！";
        }
        else
        {
            m_panelText.text = "黒の勝ち！";
        }
    }

    void Update()
    {
        if (m_change == true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                m_change = false;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                m_change = false;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                m_change = false;
            }
        }
    }

    /// <summary>
    /// 棋譜の実行
    /// </summary>
    /// <returns></returns>
    IEnumerator GameRecord()
    {
        int[] conversion = new int[m_GameRecord.Length];
        for (int i = 0; i < m_GameRecord.Length; i++)
        {
            char num = m_GameRecord[i];
            if (num >= '1' && num <= '8')
            {
                conversion[i] = num - '1';
            }
            else if (num >= 'a' && num <= 'h')
            {
                conversion[i] = num - 'a';
            }
        }
        for (int i = 1; i < conversion.Length; i += 2)
        {
            PieceAllChange(conversion[i - 1], conversion[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Reset()
    {
        if (!firstPush)
        {
            SceneManager.LoadScene("Reversi");
            firstPush = true;
        }
    }
}
