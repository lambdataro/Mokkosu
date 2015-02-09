type Color = Red | Blue | Green | RGB(Int, Int, Int -> Int -> Int)
and Tree<T,U> = Leaf | Node(T, Tree<T>, Tree<T>);