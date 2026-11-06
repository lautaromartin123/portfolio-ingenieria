CREATE TABLE `storage_deletions` (
	`object_key` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE INDEX `idx_cleanup_user` ON `storage_deletions` (`user_id`);
