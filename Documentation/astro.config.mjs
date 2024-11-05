import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightOpenAPI, { openAPISidebarGroups } from 'starlight-openapi';
import remarkMermaid from 'remark-mermaidjs'
import expressiveCode from 'astro-expressive-code';

export default defineConfig({
    site: 'https://kelo221.github.io',
    base: '/fs19_CSharp_Teamwork',

    markdown: {
        // Applied to .md and .mdx files
        remarkPlugins: [remarkMermaid],
    },
    integrations: [expressiveCode(), starlight({
        title: 'My Docs',
        social: {
            github: 'https://github.com/withastro/starlight',
        },
        plugins: [
            starlightOpenAPI([
                {
                    base: 'api',
                    label: 'My API',
                    schema: './src/content/schemas/swagger.yaml',
                    slugPrefix: 'api/operations/',
                    operationIdMap: {
                        'boards.{board_id}.lists-post': 'create-board-list',
                        'lists.{list_id}.cards-post': 'create-list-card',
                        'boards.{board_id}.members-post': 'add-board-member'
                    },
                    // Configure how operation IDs are generated
                    operationIds: {
                        clean: (operationId) => {
                            return operationId
                                .replace(/\./g, '-')
                                .replace(/{|}/g, '')
                                .toLowerCase();
                        }
                    }
                },
            ]),
        ],
        sidebar: [
            ...openAPISidebarGroups,
        ],
    }), expressiveCode()],
});