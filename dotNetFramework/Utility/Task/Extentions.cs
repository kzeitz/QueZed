using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility {
	public static class TaskExtensions {
		public static async Task Then(this Task antecedent, Action continuation) { await antecedent; continuation(); }
		public static async Task Then(this Task task, Func<Task> continuation) { await task; await continuation(); }

		public static async Task Then<TResult>(this Task<TResult> antecedent, Action<TResult> continuation) { continuation(await antecedent); }
		public static async Task Then<TResult>(this Task<TResult> task, Func<TResult, Task> continuation) { await continuation(await task); }

		public static async Task<TNewResult> Then<TNewResult>(this Task antecedent, Func<TNewResult> continuation) { await antecedent; return continuation(); }
		public static async Task<TNewResult> Then<TNewResult>(this Task task, Func<Task<TNewResult>> continuation) { await task; return await continuation(); }

		public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> antecedent, Func<TResult, TNewResult> continuation) { return continuation(await antecedent); }
		public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, Task<TNewResult>> continuation) { return await continuation(await task); }

		public static Task Sequence(params Func<Task>[] actions) {
			Task last = null;
			foreach (var action in actions) last = (last == null) ? Task.Factory.StartNew(action).Unwrap() : last.Then(action);
			return last;
		}
	}
}
